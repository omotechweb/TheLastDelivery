# The Last Delivery — Mod Kiti

Oyun açılırken şu klasörlerdeki her alt klasörü bir mod olarak yükler:

| Konum | Açıklama |
|---|---|
| `%LocalAppData%\TheLastDelivery\Mods\<mod>\` | Elle kurulan modlar |
| `<oyun klasörü>\Mods\<mod>\` | Oyunla birlikte dağıtılan modlar |
| Steam Workshop | Abone olunan öğelerin klasörleri (Steam sürümünde) |
| `TLD_MODS_PATH` ortam değişkeni | Mod geliştirirken test klasörü |

Yüklenen modlar ve hataları ana menüdeki **MODLAR** ekranında görünür. Hatalı bir mod oyunu
durdurmaz; geçerli kısımları yine yüklenir.

## Dosyalar

- `mod.json` — skinler, diyaloglar, duraklar ve metinler (bkz. `ExampleMod/mod.json`).
- `strings.<dil>.txt` — `anahtar = metin` satırları. Diller: `tr`, `en`, `es`, `zh`, `hi`, `ar`.

## mod.json alanları

- `id` — harf, rakam, `_` ve `-` (en fazla 40 karakter). Diğer kimlikler de aynı kurala uyar.
- `skins[]` — `id`, `price`, `name` (dil → metin), `style`, `colors` (`#RRGGBB`):
  `wagonBody`, `wagonPlank`, `iron`, `boiler`, `boilerBand`, `cabBody`, `roof`, `shine`, `trim`.
  Skinler Skinler ekranında `mod_<modId>_<skinId>` kimliğiyle listelenir.
- `dialogs[]` — yabancılı istasyonlarda rastgele çıkar. `title`, `text`, `a`, `b`.
  Seçenek alanları: `label`, `outcome`, `fuel` (0–60), `health` (0–60), `karma` (−50…50),
  `sanity` (−50…50), `rations` (−5…5), `takeStranger`.
  Metinde `{0}` yabancının adı, `{1}` mesleğidir.
- `stops[]` — `km` (10…1990) ve `kind` (`depot` ya da `station`). Var olan bir durağa 6 km'den
  yakın duraklar atlanır.
- `strings` — dil kodu → (anahtar → metin). Oyunun her metni değiştirilebilir ya da yeni
  anahtarlar eklenebilir.

## Not

Piksel yazı tipi Türkçe, İngilizce ve İspanyolca karakterleri destekler; `#`, `&`, `\` gibi
işaretler yoktur. Çince, Hintçe ve Arapça metinler sistem yazı tipiyle çizilir.
