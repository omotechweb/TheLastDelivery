using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
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

        Speaker speaker = _node.SpeakerKey != null
            ? new Speaker(Loc.T(_node.SpeakerKey), _node.Portrait) { IsPlayer = _node.Portrait == PortraitKind.Arthur }
            : null;
        string title = _node.Title != null ? Loc.T(_node.Title) : speaker?.Name ?? _contextTitle;
        string text = _node.Text + (summary.Count > 0 ? "\n\n" + string.Join("\n", summary) : "");
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
            var speaker = _node.SpeakerKey != null ? new Speaker(Loc.T(_node.SpeakerKey), _node.Portrait) : null;
            _box.Open(speaker?.Name ?? _contextTitle, summary, null, "prompt.continue", speaker);
            return;
        }
        _box.Close();
    }

    public void Draw(SpriteBatch sb, float time)
    {
        if (IsOpen) _box.Draw(sb, time);
    }
}
