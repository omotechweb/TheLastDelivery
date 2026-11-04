using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Core;
using TheLastDelivery.Economy;
using TheLastDelivery.Graphics;
using TheLastDelivery.Items;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;
using TheLastDelivery.Story;
using TheLastDelivery.UI;
using TheLastDelivery.World;

namespace TheLastDelivery;

/// <summary>
/// Derin anlatı güncellemesinin oyun tarafı: güven ağlarının sonuçları (kaçış, bıçaklama, ihbar, mahkeme hükmü),
/// belge/kaset/hatıra arşivi, yan görevler, AURA'nın zihin oyunları (sahte göstergeler, sahte duraklar, ölülerin
/// sesini taklit eden yayınlar), hikâye yayınları, dinamik hava cepheleri, mini harita ve yolcularla serbest sohbet.
/// </summary>
public partial class Game1
{
    private readonly ArchivePanel _archive = new();
    private readonly AuraMindGames _mind = new();
    private readonly List<MiniMapMark> _mapMarks = new();
    private readonly HashSet<string> _talkedThisChapter = new();
    private float _reportAmbushTimer = -1f;
    private int _talkRotation;
    private int _whisperIndex;
    private bool _gaugeWarned;
    private float _frameDt;
    private readonly CinematicPlayer _cinematic = new();

    /// <summary>Bölümün açılış sinematiği: Episode 1 "e1.{n}", Episode 2 "e2.{n}" (diğer modlarda yok).</summary>
    private string CinematicId(Chapter chapter) => chapter.Kind switch
    {
        ChapterKind.Story when IsStoryMode => $"e1.{chapter.Number}",
        ChapterKind.Episode2 => $"e2.{chapter.Number}",
        _ => null
    };

    /// <summary>
    /// Bölüm kartından sonra: bölümün uzun anlatım sahnesi bu hikâyede henüz izlenmediyse önce o oynar, bitince
    /// sürüş ya da hikâye sahnesi başlar. İzlenen sahne yeniden oynamaz (yeni hikâyede yeniden açılır).
    /// </summary>
    private void BeginChapterPlay()
    {
        void Go()
        {
            if (_state != GameState.ChapterIntro) return;
            if (IsStoryMode && CurrentChapter.IsStoryScene) EnterStoryScene();
            else ChangeState(GameState.Driving);
        }

        string scene = CinematicId(CurrentChapter);
        string seen = "cs:" + scene;
        string title = Loc.F("chapter.label", CurrentChapter.Number) + "  -  " + CurrentChapter.Name;
        if (scene != null && !Narrative.Seen(seen) && _cinematic.Open(scene, title, Go))
        {
            Narrative.MarkSeen(seen);
            return;
        }
        Go();
    }

    private int Episode => _mode == GameMode.Episode2 ? 2 : 1;

    /// <summary>Anlatılı bir bölümde miyiz (Episode 1 ya da Episode 2 hattı)?</summary>
    private bool InNarrativeChapter => IsStoryMode && CurrentChapter.Kind is ChapterKind.Story or ChapterKind.Episode2;

    private void ResetDeep()
    {
        _archive.Close();
        _mind.Reset();
        DialogueTreeManager.ResetRun();
        _reportAmbushTimer = -1f;
        _talkedThisChapter.Clear();
        _gaugeWarned = false;
        Hud.TemperatureLie = Hud.SpeedLie = CabinPanel.FuelLie = 0f;
    }

    /// <summary>Yeni bölüm: hava cepheleri tohumlanır, bölümün hikâye yayını telsize düşer, zihin oyunları sıfırlanır.</summary>
    private void StartChapterDeep()
    {
        _weather.StartChapter(CurrentChapter.Number + (int)_mode * 100, InNarrativeChapter && !CurrentChapter.IsStoryScene,
            GameConfig.WeatherFrontMin, GameConfig.WeatherFrontMax);
        _mind.StartChapter();
        _talkedThisChapter.Clear();
        _gaugeWarned = false;
        _radio.SetChannel(RadioChannelKind.Phantom, null);
        _radio.SetChannel(RadioChannelKind.Broadcast, InNarrativeChapter ? BroadcastKey() : null);
    }

    /// <summary>
    /// Bölümün hikâye yayını: bu bölüme yazılmış yayın ("bc.{ep}.{n}") yoksa sıradaki genel yayın ("bc.{ep}.g{n}").
    /// Dinlenen yayınlar arşive kaset olarak girer.
    /// </summary>
    private string BroadcastKey()
    {
        string specific = $"bc.{Episode}.{CurrentChapter.Number}";
        if (Loc.Has(specific)) return specific;
        for (int i = 0; i < 12; i++)
        {
            string generic = $"bc.{Episode}.g{(CurrentChapter.Number + i) % 12 + 1}";
            if (Loc.Has(generic) && !LoreArchive.Has(generic)) return generic;
        }
        return null;
    }

    // ------------------------------------------------------------------
    // Diyalog etkileri ve sonuçları
    // ------------------------------------------------------------------

    /// <summary>Güven, arşiv, yan görev ve AURA tavrı etkileri. "{p}" NPC'si diyaloğun açıldığı yolcudur.</summary>
    private void ApplyDeep(NarrativeEffect e, Passenger questPassenger, List<string> lines)
    {
        void Add(string line)
        {
            if (!string.IsNullOrEmpty(line)) lines.Add(line);
        }

        string npc = e.Npc == "{p}" ? questPassenger?.Profile.Id : e.Npc ?? DialogueTreeManager.Speaking ?? questPassenger?.Profile.Id;
        if (e.Trust != 0 && npc != null) Add(DialogueTreeManager.Adjust(npc, e.Trust));
        if (e.Consequence != null)
        {
            string target = e.Npc == null || e.Npc == "{p}" ? questPassenger?.Profile.Id ?? DialogueTreeManager.Speaking : e.Npc;
            DialogueTreeManager.Queue(target, e.Consequence);
        }
        if (e.Doc != null) Add(LoreArchive.Collect(e.Doc));
        if (e.StartQuest != null) Add(SideQuests.Start(e.StartQuest));
        if (e.FinishQuest != null) Add(SideQuests.Finish(e.FinishQuest));
        if (e.FailQuest != null) Add(SideQuests.Fail(e.FailQuest));
        if (e.Aura != 0) Add(DialogueTreeManager.AdjustStance(e.Aura));
        if (e.SetFlag == Flags.ArmorPlated || e.SetFlag2 == Flags.ArmorPlated) ApplyProfile(); // Zırh hemen işlesin
    }

    private Passenger FindPassenger(string id)
    {
        if (id == null) return null;
        foreach (var p in _cabin.Passengers)
            if (p.Profile.Id == id) return p;
        return null;
    }

    /// <summary>Mahkeme anlarının sanığı: gerçek hain varsa ve şüphe ona yöneldiyse o; yoksa en şüpheli yolcu.</summary>
    private Passenger MostSuspicious()
    {
        Passenger best = null;
        float score = -1f;
        foreach (var p in _cabin.Passengers)
        {
            if (p.IsRevealed || p.Skill == PassengerSkill.Child) continue;
            float s = p.Suspicion + (p.IsTraitor ? 15f : 0f) + (100 - DialogueTreeManager.Trust(p.Profile.Id)) * 0.2f;
            if (s > score)
            {
                score = s;
                best = p;
            }
        }
        return best;
    }

    /// <summary>
    /// Güveni dibe vuran NPC'lerin tepkileri ve mahkeme hükümleri. Diyalog kapandıktan sonra bir kez uygulanır.
    /// </summary>
    private void ResolveConsequences()
    {
        foreach (var c in DialogueTreeManager.TakePending())
        {
            var npc = DialogueTreeManager.Find(c.Npc);
            var passenger = FindPassenger(c.Npc);
            string name = passenger?.Name ?? npc?.Name ?? c.Npc;
            switch (c.Kind)
            {
                case "flee":
                    if (passenger != null)
                    {
                        _cabin.Remove(passenger);
                        int took = 0;
                        for (int i = 0; i < 2; i++) if (_inventory.TryConsumeRation()) took++;
                        _abandonedCount++;
                        _chatter.QueueReaction("left", name);
                        _toasts.Show(Loc.F("cq.flee", name, took), CabinPanel.Critical);
                        _audio.PlayError();
                    }
                    else _toasts.Show(Loc.F("cq.cold", name), Hud.Amber);
                    break;

                case "leave":
                    if (passenger != null)
                    {
                        _cabin.Remove(passenger);
                        _abandonedCount++;
                        _chatter.QueueReaction("left", name);
                        _toasts.Show(Loc.F("cq.leave", name), CabinPanel.Critical);
                    }
                    else _toasts.Show(Loc.F("cq.cold", name), Hud.Amber);
                    break;

                case "stab":
                {
                    // Arkadan bıçak: kabinde boğuşma, devrilen kazan kapağı, çalınan bir eşya
                    _train.Damage(GameConfig.StabDamage);
                    _sanity.Restore(-GameConfig.StabSanity);
                    _chapterHealthLost = true;
                    AddShake(9f);
                    string stolen = StealOneItem();
                    if (passenger != null)
                    {
                        _cabin.Remove(passenger);
                        _chatter.QueueReaction("betray", name);
                    }
                    _toasts.Show(Loc.F(passenger != null ? "cq.stab" : "cq.stab.town", name)
                                 + (stolen != null ? "  " + Loc.F("cq.stole", stolen) : ""), Hud.Danger);
                    _audio.PlayAlarm();
                    break;
                }

                case "report":
                    // Yerimizi ihbar etti: Episode 2'de AURA'nın kulelerine, Episode 1'de Kızıl Birlik'e
                    Narrative.Set(Flags.Reported);
                    _reportAmbushTimer = GameConfig.ReportAmbushDelay;
                    if (passenger != null)
                    {
                        _cabin.Remove(passenger);
                        _chatter.QueueReaction("betray", name);
                    }
                    _toasts.Show(Loc.F(Episode == 2 ? "cq.report2" : "cq.report1", name), Hud.Danger);
                    _audio.PlayAlarm();
                    break;

                case "handover":
                    // Yolcu ödül avcısına teslim edildi
                    if (passenger != null)
                    {
                        _cabin.Remove(passenger);
                        _abandonedCount++;
                        _chatter.QueueReaction("left", name);
                        _toasts.Show(Loc.F("cq.handover", name), CabinPanel.Critical);
                        _audio.PlayDeath();
                    }
                    break;

                case "cold":
                    _toasts.Show(Loc.F("cq.cold", name), Hud.Amber);
                    break;

                // ---- Mahkeme hükümleri (sanık: diyaloğun yolcusu) ----
                case "verdict.vote":
                    if (passenger == null) break;
                    if (passenger.IsTraitor)
                    {
                        passenger.Reveal();
                        EjectPassenger(passenger);
                        _toasts.Show(Loc.F("cq.vote.guilty", name), CabinPanel.Good);
                    }
                    else
                    {
                        passenger.AddSuspicion(-100f);
                        DialogueTreeManager.Adjust(passenger.Profile.Id, 15);
                        _toasts.Show(Loc.F("cq.vote.innocent", name), Hud.Amber);
                    }
                    break;

                case "verdict.exile":
                    if (passenger != null) EjectPassenger(passenger);
                    break;

                case "verdict.execute":
                    if (passenger == null) break;
                    bool guilty = passenger.IsTraitor;
                    EjectPassenger(passenger);
                    _karma -= 20;
                    _sanity.Restore(-15f);
                    foreach (var p in _cabin.Passengers) p.LiftMorale(guilty ? -5f : -20f);
                    _toasts.Show(Loc.F(guilty ? "cq.execute.guilty" : "cq.execute.innocent", name), Hud.Danger);
                    break;

                case "verdict.acquit":
                    if (passenger == null) break;
                    passenger.AddSuspicion(-30f);
                    if (!passenger.IsTraitor) DialogueTreeManager.Adjust(passenger.Profile.Id, 10);
                    _toasts.Show(Loc.F("cq.acquit", name), Hud.Label);
                    break;
            }
        }
    }

    /// <summary>Bıçaklayan kaçarken çantadan bir eşya kapar (varsa); adını döndürür.</summary>
    private string StealOneItem()
    {
        foreach (ItemType type in new[] { ItemType.Medkit, ItemType.ThermalCoal, ItemType.SpareParts })
            if (_inventory.TryConsume(type)) return ItemCatalog.Name(type);
        return null;
    }

    // ------------------------------------------------------------------
    // Yolcularla serbest sohbet ([J], görev adımı yoksa)
    // ------------------------------------------------------------------

    /// <summary>
    /// Sırayla bir yolcuyla konuşulur: önce yolcuya özel sohbet ağı ("talk.{id}.1"), o bittiyse genel sohbet ağı.
    /// Her yolcuyla bölüm başına bir kez (güven kasılamasın).
    /// </summary>
    private bool TryOpenTalk()
    {
        if (!IsStoryMode || _cabin.Count == 0)
        {
            _toasts.Show(Loc.T("quest.none"), Hud.Label);
            return false;
        }
        for (int i = 0; i < _cabin.Count; i++)
        {
            var p = _cabin.Passengers[(_talkRotation + i) % _cabin.Count];
            string id = p.Profile.Id;
            if (_talkedThisChapter.Contains(id) || p.IsRevealed) continue;
            _talkRotation = (_talkRotation + i + 1) % Math.Max(1, _cabin.Count);
            _talkedThisChapter.Add(id);

            string node = "talk.hub";
            for (int part = 1; part <= 3; part++)
            {
                string personal = $"talk.{id}.{part}";
                if (NarrativeData.Node(personal) == null) break;
                if (!Narrative.Seen(personal) && TalkPartUnlocked(part)) { node = personal; break; }
            }
            _audio.PlayClick();
            _story.Open(this, node, p.Name, p);
            return true;
        }
        _toasts.Show(Loc.T("talk.none"), Hud.Label);
        return false;
    }

    /// <summary>Kişisel sohbetin parçaları bölümler ilerledikçe açılır (yolcu açılmak için zaman ister).</summary>
    private bool TalkPartUnlocked(int part) => part == 1 || CurrentChapter.Number >= part * 8 - 4;

    // ------------------------------------------------------------------
    // Telsiz: hikâye yayınları ve AURA'nın ses taklitleri
    // ------------------------------------------------------------------

    private void HandleRadioDeep(RadioAction action)
    {
        switch (action)
        {
            case RadioAction.BroadcastHeard:
            {
                string key = _radioPanel.Channel?.Key;
                string line = key != null ? LoreArchive.Collect(key) : null;
                if (line != null) _toasts.Show(line + "  " + Loc.T("arc.open"), new Color(200, 230, 255));
                break;
            }

            case RadioAction.PhantomDrain:
                // Ölülerin sesini dinlemek aklı aşındırır
                _sanity.Restore(-1.6f * _frameDt);
                break;

            case RadioAction.AnswerPhantom:
            {
                _radioPanel.Close();
                _mind.SilenceRadio();
                _radio.SetChannel(RadioChannelKind.Phantom, null);
                var spot = NarrativeData.Spot(Episode == 2 ? "ph.voices2" : "ph.voices1");
                if (spot != null) _story.Open(this, Narrative.NextEntry(spot), Loc.T("radio.phantom"));
                break;
            }
        }
    }

    /// <summary>Ses taklidi yayınının telsizde görünen kısa metni (akıl sağlığı düştükçe daha kişisel).</summary>
    private string PhantomKey()
    {
        int n = _sanity.Sanity < 25f ? 4 : _sanity.Sanity < 40f ? 3 : Episode == 2 ? 2 : 1;
        return $"ph.radio.{n}";
    }

    // ------------------------------------------------------------------
    // Her kare
    // ------------------------------------------------------------------

    private void UpdateDeep(float dt, InputState input)
    {
        // [N]: mini harita
        if (IsTravelling && input.Pressed(Keys.N))
        {
            SaveData.Current.MiniMapHidden = !SaveData.Current.MiniMapHidden;
            SaveData.Save();
            _audio.PlayClick();
        }

        bool driving = (IsTravelling || _state is GameState.Chase or GameState.Sentinel) && !CurrentChapterIsScene;
        if (InNarrativeChapter && IsInJourney)
            _mind.Update(dt, driving, Episode, CurrentChapter.Number, _sanity.Sanity, _travelledKm, DialogueTreeManager.Stance);
        else
            _mind.Update(dt, false, Episode, 0, 100f, _travelledKm, 0f);

        Hud.TemperatureLie = _mind.TemperatureLie;
        Hud.SpeedLie = _mind.SpeedLie;
        CabinPanel.FuelLie = _mind.FuelLie;

        // Telsizdeki ses taklidi kanalı zihin oyunlarıyla birlikte belirir ve kaybolur
        if (_mind.PhantomRadio != _radio.HasChannel(RadioChannelKind.Phantom))
            _radio.SetChannel(RadioChannelKind.Phantom, _mind.PhantomRadio ? PhantomKey() : null);

        switch (_mind.Event)
        {
            case "mind.gauge" when !_gaugeWarned:
                _gaugeWarned = true;
                _toasts.Show(Loc.T("mind.gauge"), Hud.Danger);
                _audio.PlayEerie();
                break;
            case "mind.radio":
                _toasts.Show(Loc.T(Episode == 2 ? "mind.radio2" : "mind.radio1"), new Color(255, 120, 130));
                break;
            case "mind.phantomGone":
                _toasts.Show(Loc.T("mind.phantomGone"), Hud.Label);
                break;
            case "mind.whisper":
            {
                string key = Episode == 2 ? $"mind.whisper.{_whisperIndex % 12 + 1}" : $"mind.echo.{_whisperIndex % 6 + 1}";
                if (_chatter.Say(Loc.T(Episode == 2 ? "chat.aura" : "chat.unknown"), Loc.T(key), ChatterKind.Radio, aura: true))
                {
                    _whisperIndex++;
                    _audio.PlayEerie();
                }
                break;
            }
        }

        // Hava cephesi değişti: telsizden kısa anons
        if (_weather.FrontChanged is { } front && IsTravelling && InNarrativeChapter)
        {
            if (!_chatter.Say(Loc.T("chat.radio"), Loc.T($"wx.front.{front}"), ChatterKind.Radio, aura: false))
                _toasts.Show(Loc.T($"wx.front.{front}"), new Color(170, 200, 255));
        }

        // İhbar: bir süre sonra pusu kurulur (Episode 2'de Sentinel, Episode 1'de Kızıl Birlik)
        if (_reportAmbushTimer > 0f && _state == GameState.Driving && !CurrentChapterIsScene)
        {
            _reportAmbushTimer -= dt;
            if (_reportAmbushTimer <= 0f)
            {
                bool safe = _events.Active == null && NextStopKm - _travelledKm > GameConfig.ApproachKm + GameConfig.AmbushSafetyKm;
                if (!safe)
                {
                    _reportAmbushTimer = 8f; // Uygun anı bekler
                }
                else
                {
                    _toasts.Show(Loc.T(Episode == 2 ? "cq.reportHit2" : "cq.reportHit1"), Hud.Danger);
                    if (_mode == GameMode.Episode2) StartSentinel();
                    else StartChase();
                }
            }
        }
    }

    private bool CurrentChapterIsScene => CurrentChapter.IsStoryScene;

    /// <summary>Arşiv yolculuk sırasında, başka bir pencere açık değilken açılabilir.</summary>
    private bool CanOpenArchive =>
        _state is GameState.Driving or GameState.Arriving or GameState.Town or GameState.Station or GameState.Depot
        && _townDialog == TownDialog.None && !_cabinMenu.IsOpen && !_radioPanel.IsOpen && !_garage.IsOpen
        && !_hack.IsOpen && _fadeTimer < 0f;

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    /// <summary>Sis cephesi: sahnenin üstünde, arayüzün altında yumuşak gri perde.</summary>
    private void DrawFog()
    {
        if (_weather.Fog < 0.01f || !InNarrativeChapter) return;
        float wave = 0.85f + 0.15f * MathF.Sin(_time * 0.6f);
        _spriteBatch.FillRect(new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight),
            new Color(150, 158, 172) * (_weather.Fog * 0.5f * wave));
    }

    /// <summary>Mini harita: bölümün hattı, duraklar, imdat noktası, AURA kuleleri, hikâye anları ve sahte duraklar.</summary>
    private void DrawMiniMap()
    {
        if (!InNarrativeChapter || SaveData.Current.MiniMapHidden) return;
        var chapter = CurrentChapter;
        float span = chapter.EndKm - chapter.StartKm;
        _mapMarks.Clear();
        foreach (var stop in Stops)
        {
            if (stop.Km < chapter.StartKm || stop.Km > chapter.EndKm + span * 0.5f) continue;
            _mapMarks.Add(new MiniMapMark(stop.Km, stop.Kind switch
            {
                StopKind.Town => MapMark.Town,
                StopKind.Depot => MapMark.Depot,
                StopKind.Hub => MapMark.Hub,
                StopKind.Shelter => MapMark.Shelter,
                _ => MapMark.Station
            }));
        }
        if (_sosKm > 0f) _mapMarks.Add(new MiniMapMark(_sosKm, MapMark.Sos));
        if (_towerKm > 0f) _mapMarks.Add(new MiniMapMark(_towerKm, MapMark.Tower));
        foreach (float at in StoryBeats.Upcoming(Episode, chapter.Number))
            _mapMarks.Add(new MiniMapMark(chapter.StartKm + at * span, MapMark.Story));
        if (_mind.PhantomKm > 0f) _mapMarks.Add(new MiniMapMark(_mind.PhantomKm, MapMark.Phantom));

        RouteMiniMap.Draw(_spriteBatch, chapter.StartKm, chapter.EndKm, _travelledKm, _mapMarks, NextStopKm, StopName(NextStop),
            Loc.T($"wx.now.{_weather.Front}"), _time, _mind.Strength);
    }

    private static string StopName(RouteStop stop) => stop.Kind switch
    {
        StopKind.Town => TownCatalog.All[stop.Index].Name,
        StopKind.Depot => Loc.T("map.stop.depot"),
        StopKind.Hub => Loc.T("map.stop.hub"),
        StopKind.Shelter => Loc.T("map.stop.shelter"),
        _ => Loc.T("map.stop.station")
    };
}
