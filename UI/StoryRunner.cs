using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastDelivery.Graphics;
using Microsoft.Xna.Framework.Input;
using TheLastDelivery.Audio;
using TheLastDelivery.Core;
using TheLastDelivery.Localization;
using TheLastDelivery.Passengers;
using TheLastDelivery.Story;

namespace TheLastDelivery.UI;

/// <summary>Hikâye düğümlerinin oyuna etkisini uygulayan taraf (Game1).</summary>
public interface INarrativeHost
{
    /// <summary>Seçenek şu an seçilebilir mi? (Kredi, eşya, erzak, boş yatak...)</summary>
    bool CanApply(NarrativeEffect effect, Passenger questPassenger);

    /// <summary>Etkiyi uygular; oyuncuya gösterilecek özet satırlarını döndürür.</summary>
    string Apply(NarrativeEffect effect, Passenger questPassenger);

    /// <summary>Diyalog bölümünün sahnesi tamamlandı.</summary>
    void OnSceneComplete();
}

/// <summary>
/// Diyalog ve inceleme düğümlerini oynatır: metin daktiloyla akar, [1]-[4] seçer, [BOŞLUK] devam eder.
/// Seçimler bayrak olarak kaydedilir ve sonraki bölümleri değiştirir. Açıkken oyun zamanı durur.
/// </summary>
public sealed class StoryRunner
{
    private readonly DialogBox _box = new();
    private readonly List<NarrativeOption> _shown = new();
    private INarrativeHost _host;
    private NarrativeNode _node;
    private Passenger _passenger;
    private string _contextTitle;
    private bool _showingResult;
    private bool _completePending;
    private string _trustNpc;       // Güven barı gösterilen kişi (yoksa null)
    private float _trustShown;      // Bar yumuşakça yeni değere kayar
    private float _lastDraw;

    public bool IsOpen { get; private set; }

    /// <param name="contextTitle">Konuşan yoksa pencere başlığı (inceleme noktasının ya da sahnenin adı).</param>
    /// <param name="questPassenger">Görev diyaloğunda konuşulan yolcu (moral ve şüphe etkileri ona uygulanır).</param>
    public void Open(INarrativeHost host, string nodeId, string contextTitle, Passenger questPassenger = null)
    {
        _host = host;
        _contextTitle = contextTitle;
        _passenger = questPassenger;
        _completePending = false;
        IsOpen = true;
        Show(nodeId, null);
    }

    private void Show(string nodeId, string carriedSummary)
    {
        _node = NarrativeData.Node(nodeId);
        if (_node == null)
        {
            _box.Close();
            return;
        }

        Narrative.MarkSeen(_node.Id);
        _trustNpc = _node.Npc == "{p}" ? _passenger?.Profile.Id : _node.Npc ?? DialogueTreeManager.NpcForSpeaker(_node.SpeakerKey)
                    ?? (_node.SpeakerKey == null ? null : _passenger?.Profile.Id);
        DialogueTreeManager.Speaking = _trustNpc ?? _passenger?.Profile.Id;
        _trustShown = _trustNpc != null ? DialogueTreeManager.Trust(_trustNpc) : 0;
        var summary = new List<string>();
        if (!string.IsNullOrEmpty(carriedSummary)) summary.Add(carriedSummary);
        if (_node.OnEnter != null)
        {
            string entered = _host.Apply(_node.OnEnter, _passenger);
            if (!string.IsNullOrEmpty(entered)) summary.Add(entered);
            if (_node.OnEnter.CompleteScene) _completePending = true;
        }
        SaveData_Save();

        _shown.Clear();
        var options = new List<DialogOption>();
        for (int index = 0; index < _node.Options.Length; index++)
        {
            var option = _node.Options[index];
            if (option.RequireFlag != null && !Narrative.Has(option.RequireFlag)) continue;
            if (option.ForbidFlag != null && Narrative.Has(option.ForbidFlag)) continue;
            if (_shown.Count >= 4) break; // Yalnızca 1-4 tuşları seçer; fazlası sonraki açılışta sıraya girer
            bool enabled = option.Effect == null || _host.CanApply(option.Effect, _passenger);
            _shown.Add(option);
            options.Add(new DialogOption((_shown.Count).ToString(), _node.OptionLabel(index), enabled));
        }

        // "{p}": konuşan, diyaloğun açıldığı yolcudur (genel sohbet ağları, mahkemedeki sanık)
        Speaker speaker = _node.SpeakerKey == "{p}" && _passenger != null
            ? new Speaker(_passenger.Name, Portraits.ForPassenger(_passenger.Profile.Id))
            : _node.SpeakerKey == "{p}" ? null
            : _node.SpeakerKey != null
            ? new Speaker(Loc.T(_node.SpeakerKey), _node.Portrait) { IsPlayer = _node.Portrait == PortraitKind.Arthur }
            : null;
        string title = _node.Title != null ? Loc.T(_node.Title) : speaker?.Name ?? _contextTitle;
        string body = _passenger != null ? _node.Text.Replace("{p}", _passenger.Name) : _node.Text;
        string text = body + (summary.Count > 0 ? "\n\n" + string.Join("\n", summary) : "");
        _showingResult = false;
        _box.Open(title, text, options.Count > 0 ? options : null, "prompt.continue", speaker);
    }

    private static void SaveData_Save() => Economy.SaveData.Save();

    public void Update(float dt, InputState input, ProceduralAudio audio)
    {
        if (!IsOpen) return;
        _box.Update(dt);

        if (_box.IsClosed)
        {
            IsOpen = false;
            if (_completePending)
            {
                _completePending = false;
                _host.OnSceneComplete();
            }
            return;
        }

        if (input.Pressed(Keys.Space) && !_box.IsComplete)
        {
            _box.Skip();
            return;
        }

        bool hasChoices = !_showingResult && _shown.Count > 0;
        if (!hasChoices)
        {
            if (_box.IsComplete && input.Pressed(Keys.Space))
            {
                audio.PlayClick();
                if (!_showingResult && _node?.Next != null) Show(_node.Next, null);
                else _box.Close();
            }
            return;
        }

        if (!_box.AcceptsChoice) return;
        for (int i = 0; i < _shown.Count && i < 4; i++)
        {
            if (!input.AnyPressed((Keys)((int)Keys.D1 + i), (Keys)((int)Keys.NumPad1 + i))) continue;
            Choose(_shown[i], audio);
            return;
        }
    }

    private void Choose(NarrativeOption option, ProceduralAudio audio)
    {
        if (option.Effect != null && !_host.CanApply(option.Effect, _passenger))
        {
            audio.PlayError();
            return;
        }
        audio.PlayConfirm();
        DialogueTreeManager.RecordChoice(_node.Id, Array.IndexOf(_node.Options, option));

        string summary = option.Effect != null ? _host.Apply(option.Effect, _passenger) : null;
        if (option.Effect?.CompleteScene == true) _completePending = true;
        SaveData_Save();

        if (option.Next != null)
        {
            Show(option.Next, summary);
            return;
        }

        if (!string.IsNullOrEmpty(summary))
        {
            // Sonuç: seçeneğin oyuna etkisi kısa bir pencerede
            _showingResult = true;
            var speaker = _node.SpeakerKey == "{p}"
                ? (_passenger != null ? new Speaker(_passenger.Name, Portraits.ForPassenger(_passenger.Profile.Id)) : null)
                : _node.SpeakerKey != null ? new Speaker(Loc.T(_node.SpeakerKey), _node.Portrait) : null;
            _box.Open(speaker?.Name ?? _contextTitle, summary, null, "prompt.continue", speaker);
            return;
        }
        _box.Close();
    }

    public void Draw(SpriteBatch sb, float time)
    {
        if (!IsOpen) return;
        _box.Draw(sb, time);
        DrawTrust(sb, time);
    }

    /// <summary>
    /// Konuşulan kişinin güven/şüphe barı (pencerenin üstünde). AURA ile konuşurken AURA'nın Arthur'a tavrı gösterilir.
    /// Değer yumuşakça yeni düzeye kayar; seçimin etkisi böylece görülür ama sayı olarak verilmez.
    /// </summary>
    private void DrawTrust(SpriteBatch sb, float time)
    {
        if (_trustNpc == null) return;
        bool aura = _trustNpc == "aura";
        float target = aura ? (DialogueTreeManager.Stance + 100) / 2f : DialogueTreeManager.Trust(_trustNpc);
        float dt = Math.Clamp(time - _lastDraw, 0f, 0.1f);
        _lastDraw = time;
        _trustShown += (target - _trustShown) * Math.Clamp(dt * 4f, 0f, 1f);

        var panel = new Rectangle(770, 8, 340, 38);
        sb.FillRect(panel, new Color(5, 6, 12) * 0.82f);
        sb.DrawBorder(panel, 2, new Color(80, 90, 115));
        string label = aura ? Loc.F("trust.aura", DialogueTreeManager.StanceName)
                            : Loc.F("trust.label", DialogueTreeManager.LevelName((int)MathF.Round(target)));
        GameText.Draw(sb, label, new Vector2(panel.X + 10, panel.Y + 4), Hud.Label);
        var bar = new Rectangle(panel.X + 10, panel.Y + 26, panel.Width - 20, 6);
        sb.FillRect(bar, Color.Black * 0.7f);
        float ratio = MathHelper.Clamp(_trustShown / 100f, 0f, 1f);
        var fill = ratio < 0.35f ? Color.Lerp(new Color(230, 70, 60), new Color(240, 180, 70), ratio / 0.35f)
                                 : Color.Lerp(new Color(240, 180, 70), new Color(110, 220, 130), (ratio - 0.35f) / 0.65f);
        if (aura) fill = Color.Lerp(new Color(255, 50, 60), new Color(120, 200, 255), ratio);
        sb.FillRect(new Rectangle(bar.X, bar.Y, (int)(bar.Width * ratio), bar.Height), fill);
        // Eşik çentikleri: düşmanlık ve sadakat
        foreach (int mark in new[] { DialogueTreeManager.Hostile, DialogueTreeManager.Loyal })
            sb.FillRect(new Rectangle(bar.X + bar.Width * mark / 100, bar.Y - 2, 1, bar.Height + 4), Color.White * 0.45f);
    }
}
