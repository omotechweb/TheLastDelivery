// =====================================================================
//  DENEME KODU — OYUN YAYINLANMADAN ÖNCE KALDIRILACAK
//  Kaldırmak için: bu dosyayı silin ve "DevCheats" / "DevUnlockAll" geçen satırları temizleyin
//  (SaveData, DLCManager, MetaMenu, Game1). Derleyici kalan yerleri hata olarak gösterir.
// =====================================================================

using TheLastDelivery.Platform;
using TheLastDelivery.Story;

namespace TheLastDelivery.Economy;

/// <summary>
/// Ana menüdeki kod alanı. Doğru kod girilince: bütün DLC'ler, skinler, temalar, mikro kozmetikler,
/// geliştirmeler ve bölümler açılır; bakiye sınırsız olur. Durum kayıtta kalıcıdır.
/// </summary>
public static class DevCheats
{
    private const string UnlockCode = "deneme";
    public const int InfiniteCredits = 999999;

    public static bool IsActive => SaveData.Current.DevUnlockAll;

    [System.Runtime.InteropServices.DllImport("SDL2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
    private static extern System.IntPtr SDL_GetClipboardText();

    [System.Runtime.InteropServices.DllImport("SDL2", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
    private static extern void SDL_free(System.IntPtr memory);

    /// <summary>Panodaki metin (MonoGame DesktopGL'in kullandığı SDL2 üzerinden). Okunamazsa boş.</summary>
    public static string ReadClipboard()
    {
        try
        {
            var ptr = SDL_GetClipboardText();
            if (ptr == System.IntPtr.Zero) return "";
            string text = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(ptr) ?? "";
            SDL_free(ptr);
            return text;
        }
        catch (System.Exception)
        {
            return ""; // SDL yoksa yapıştırma sessizce çalışmaz, oyun etkilenmez
        }
    }

    /// <returns>Kod doğruysa true (her şey açılır).</returns>
    public static bool TryApply(string code)
    {
        if (code?.Trim().ToLowerInvariant() != UnlockCode) return false;

        var save = SaveData.Current;
        save.DevUnlockAll = true;
        save.Credits = InfiniteCredits;
        save.UnlockedChapter = SaveData.MaxChapter;
        save.WastelandUnlocked = Chapters.Wasteland.Length;

        foreach (var skin in Cosmetics.TrainSkins)
            if (!save.OwnedSkins.Contains(skin.Id)) save.OwnedSkins.Add(skin.Id);
        foreach (var theme in Cosmetics.CabinThemes)
            if (!save.OwnedThemes.Contains(theme.Id)) save.OwnedThemes.Add(theme.Id);
        foreach (var item in CosmeticCatalog.MicroItems)
            if (!save.OwnedCosmetics.Contains(item.Key)) save.OwnedCosmetics.Add(item.Key);
        foreach (var upgrade in UpgradeCatalog.All)
            if (!save.Upgrades.Contains(upgrade.ToString())) save.Upgrades.Add(upgrade.ToString());

        DLCManager.Refresh(); // DevUnlockAll ile 4 DLC sahip sayılır ve DLC kozmetikleri envantere eklenir
        SaveData.Save();
        return true;
    }
}
