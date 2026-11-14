using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLastDelivery.Graphics;

/// <summary>
/// Manzara katmanlarının dokuları. Hepsi "aydınlatma haritası" gibi gri tonlarda üretilir (kar = beyaz,
/// kaya ve ağaç = koyu gri); gerçek renk çizim anında günün saatine göre verilen tonla çarpılarak oluşur.
/// Bütün katmanlar periyodik gürültüyle üretildiği için yatayda dikişsiz döşenir.
/// </summary>
public static class SceneryTextures
{
    // ------------------------------------------------------------------
    // Periyodik gürültü
    // ------------------------------------------------------------------

    private static float Hash(int i, int seed)
    {
        unchecked
        {
            uint h = (uint)(i * 374761393 + seed * 668265263);
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }

    /// <summary>t (0-1) boyunca <paramref name="cells"/> hücreli, uçları birleşen yumuşak gürültü.</summary>
    private static float Noise(float t, int cells, int seed)
    {
        float x = t * cells;
        int i = (int)MathF.Floor(x);
        float f = x - i;
        f = f * f * (3f - 2f * f);
        int a = ((i % cells) + cells) % cells, b = (a + 1) % cells;
        return MathHelper.Lerp(Hash(a, seed), Hash(b, seed), f);
    }

    private static float Fbm(float t, int cells, int octaves, int seed)
    {
        float sum = 0f, amp = 0.5f, total = 0f;
        for (int o = 0; o < octaves; o++)
        {
            sum += Noise(t, cells << o, seed + o * 17) * amp;
            total += amp;
            amp *= 0.5f;
        }
        return sum / total;
    }

    /// <summary>İki boyutlu, her iki yönde de döşenebilen gürültü (bulutlar için).</summary>
    private static float Noise2(float u, float v, int cellsX, int cellsY, int seed)
    {
        float x = u * cellsX, y = v * cellsY;
        int ix = (int)MathF.Floor(x), iy = (int)MathF.Floor(y);
        float fx = x - ix, fy = y - iy;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);
        float H(int cx, int cy) => Hash(((cx % cellsX) + cellsX) % cellsX + (((cy % cellsY) + cellsY) % cellsY) * 977, seed);
        return MathHelper.Lerp(MathHelper.Lerp(H(ix, iy), H(ix + 1, iy), fx), MathHelper.Lerp(H(ix, iy + 1), H(ix + 1, iy + 1), fx), fy);
    }

    private static Color Gray(float v) => new(v, v, v, 1f);

    private static Texture2D Create(GraphicsDevice gd, int w, int h, Color[] data)
    {
        var texture = new Texture2D(gd, w, h);
        texture.SetData(data);
        return texture;
    }

    // ------------------------------------------------------------------
    // Gökyüzü parçaları
    // ------------------------------------------------------------------

    /// <summary>1 piksel genişliğinde, yukarıdan aşağı saydamdan opağa geçen şerit (ufuk rengi, sis perdesi).</summary>
    public static Texture2D CreateFade(GraphicsDevice gd, int height, float power)
    {
        var data = new Color[height];
        for (int y = 0; y < height; y++)
            data[y] = Color.White * MathF.Pow(y / (float)(height - 1), power);
        return Create(gd, 1, height, data);
    }

    /// <summary>Kutup ışığı ışını: altta parlak, yukarı doğru uzun bir kuyrukla sönen dikey şerit.</summary>
    public static Texture2D CreateAuroraRay(GraphicsDevice gd, int height)
    {
        var data = new Color[height];
        for (int y = 0; y < height; y++)
        {
            float t = y / (float)(height - 1);
            float a = t < 0.86f ? MathF.Pow(t / 0.86f, 2.4f) : 1f - (t - 0.86f) / 0.14f;
            data[y] = Color.White * MathHelper.Clamp(a, 0f, 1f);
        }
        return Create(gd, 1, height, data);
    }

    /// <summary>Kraterli, kenarı yumuşak ay diski.</summary>
    public static Texture2D CreateMoon(GraphicsDevice gd, int radius)
    {
        int size = radius * 2;
        var data = new Color[size * size];
        var rng = new Random(5);
        var craters = new (float X, float Y, float R)[9];
        for (int i = 0; i < craters.Length; i++)
            craters[i] = ((float)rng.NextDouble() * 1.3f - 0.65f, (float)rng.NextDouble() * 1.3f - 0.65f, 0.08f + (float)rng.NextDouble() * 0.16f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - radius + 0.5f) / radius, ny = (y - radius + 0.5f) / radius;
                float d = MathF.Sqrt(nx * nx + ny * ny);
                if (d > 1f) continue;
                float v = 0.92f - 0.16f * MathF.Max(0f, nx * 0.7f + ny * 0.7f); // Sağ alt kenar hafif gölgede
                foreach (var c in craters)
                {
                    float cd = MathF.Sqrt((nx - c.X) * (nx - c.X) + (ny - c.Y) * (ny - c.Y));
                    if (cd < c.R) v -= 0.16f;
                    else if (cd < c.R + 0.04f && nx - c.X + ny - c.Y > 0f) v += 0.05f; // Krater kenarında ışık
                }
                float edge = MathHelper.Clamp((1f - d) * radius, 0f, 1f);
                data[y * size + x] = Gray(MathHelper.Clamp(v, 0f, 1f)) * edge;
            }
        }
        return Create(gd, size, size, data);
    }

    /// <summary>Yatayda döşenen bulut bandı: üstü ve altı saydamlaşan pamuk öbekleri.</summary>
    public static Texture2D CreateCloudBand(GraphicsDevice gd, int width, int height, int seed)
    {
        var data = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float v = y / (float)height;
            float band = MathF.Sin(v * MathHelper.Pi);                   // Üst ve alt kenarda 0
            for (int x = 0; x < width; x++)
            {
                float u = x / (float)width;
                float n = 0.55f * Noise2(u, v, 6, 2, seed) + 0.3f * Noise2(u, v, 13, 4, seed + 3) + 0.15f * Noise2(u, v, 29, 8, seed + 7);
                float a = MathHelper.Clamp((n * band - 0.3f) / 0.28f, 0f, 1f);
                // Kademeli saydamlık: piksel sanatı havası
                a = MathF.Floor(a * 5f) / 5f;
                float shade = 0.7f + 0.3f * MathHelper.Clamp((n - 0.45f) * 4f - v * 0.6f + 0.3f, 0f, 1f); // Üst yüzler aydınlık
                data[y * width + x] = Gray(shade) * a;
            }
        }
        return Create(gd, width, height, data);
    }

    // ------------------------------------------------------------------
    // Dağlar ve tepeler
    // ------------------------------------------------------------------

    /// <summary>
    /// Sıradağ: keskin zirveler, zirvelerde kar, yamaçlarda ışık ve gölge yüzleri.
    /// <paramref name="detail"/> 0 ise uzak, puslu silüet; 1 ise karlı, yüzeyli yakın dağ.
    /// </summary>
    public static Texture2D CreateRange(GraphicsDevice gd, int width, int height, int seed, int peaks, float detail)
    {
        var data = new Color[width * height];
        var elevation = new float[width];
        for (int x = 0; x < width; x++)
        {
            float t = x / (float)width;
            float ridge = 1f - MathF.Abs(2f * Fbm(t, peaks, 3, seed) - 1f);            // Keskin sırtlar
            float broad = Fbm(t, Math.Max(2, peaks / 2), 2, seed + 40);
            float rough = Noise(t, peaks * 11, seed + 90);
            elevation[x] = MathHelper.Clamp(0.58f * ridge * ridge + 0.34f * broad + 0.08f * rough, 0f, 1f);
        }
        float E(int x) => elevation[((x % width) + width) % width];

        for (int x = 0; x < width; x++)
        {
            float e = elevation[x];
            int top = height - (int)(height * (0.22f + 0.74f * e));
            float t = x / (float)width;
            for (int y = Math.Max(0, top); y < height; y++)
            {
                int depth = y - top;
                // Yamaç yüzleri: zirveden sağa-aşağı inen sırt çizgileri ışıklı ve gölgeli yüzleri ayırır
                int sx = x - (int)(depth * 0.55f);
                bool lit = E(sx + 3) - E(sx - 3) > 0f;
                float snowLine = (14f + 46f * e) * (0.55f + 0.9f * Noise(t + depth * 0.0007f, peaks * 9, seed + 7));
                bool snow = depth < snowLine * detail + 2;
                // Kayalıkta ince kar damarları
                bool streak = detail > 0.5f && depth < snowLine * 2.6f && Noise((sx % width + width) % width / (float)width, peaks * 37, seed + 21) > 0.72f;

                float v = snow ? (lit ? 1f : 0.74f) : streak ? (lit ? 0.8f : 0.6f) : (lit ? 0.56f : 0.4f);
                v = MathHelper.Lerp(0.6f, v, 0.35f + 0.65f * detail);                    // Uzak sıra düzleşir
                // Dibe doğru pus: yüzey ayrıntısı erir
                float haze = MathHelper.Clamp((y - height * 0.5f) / (height * 0.5f), 0f, 1f);
                v = MathHelper.Lerp(v, 0.78f, haze * 0.7f);
                if (depth < 1 && detail > 0.5f) v = 1f;                                  // Sırt çizgisinde ince ışık
                data[y * width + x] = Gray(v);
            }
        }
        return Create(gd, width, height, data);
    }

    /// <summary>Yumuşak karlı tepeler; sırtlarında tek tük küçük çamlar.</summary>
    public static Texture2D CreateHills(GraphicsDevice gd, int width, int height, int seed)
    {
        var data = new Color[width * height];
        var top = new int[width];
        for (int x = 0; x < width; x++)
        {
            float t = x / (float)width;
            float e = 0.7f * Fbm(t, 3, 3, seed) + 0.3f * Noise(t, 9, seed + 5);
            top[x] = height - (int)(height * (0.2f + 0.75f * e));
            for (int y = Math.Max(0, top[x]); y < height; y++)
            {
                int depth = y - top[x];
                float v = depth < 2 ? 1f : MathHelper.Lerp(0.9f, 0.62f, MathHelper.Clamp(depth / (float)height * 1.6f, 0f, 1f));
                // Rüzgârın taradığı kar kıvrımları
                if (depth > 3 && Noise(t * 1f + depth * 0.013f, 47, seed + 60) > 0.78f) v -= 0.06f;
                data[y * width + x] = Gray(v);
            }
        }

        var rng = new Random(seed);
        for (int x = rng.Next(20, 60); x < width - 8; x += rng.Next(14, 90))
        {
            int h = rng.Next(9, 20);
            int baseY = top[x] + 2;
            for (int i = 0; i < h; i++)
            {
                int half = 1 + i * 4 / h;
                for (int dx = -half; dx <= half; dx++)
                {
                    int px = x + dx, py = baseY - h + i;
                    if (px < 0 || px >= width || py < 0 || py >= height) continue;
                    data[py * width + px] = Gray(dx == -half && i % 3 == 0 ? 0.8f : 0.3f);
                }
            }
        }
        return Create(gd, width, height, data);
    }

    // ------------------------------------------------------------------
    // Ormanlar
    // ------------------------------------------------------------------

    /// <summary>
    /// Köknar ormanı. <paramref name="minH"/>-<paramref name="maxH"/> arası boylarda, kat kat dallı, dalları karlı
    /// ağaçlar; aralarında kuru, çıplak gövdeler. <paramref name="gap"/> ağaçlar arası ortalama boşluktur.
    /// </summary>
    public static Texture2D CreateForest(GraphicsDevice gd, int width, int height, int seed, int minH, int maxH, int gap, float deadRatio)
    {
        var data = new Color[width * height];
        var rng = new Random(seed);

        void Plot(int x, int y, float v)
        {
            if (y < 0 || y >= height) return;
            x = ((x % width) + width) % width;
            data[y * width + x] = Gray(v);
        }

        // Dipteki kar yığını ve çalılık
        int[] mound = new int[width];
        for (int x = 0; x < width; x++)
        {
            float t = x / (float)width;
            mound[x] = 7 + (int)(5f * Noise(t, 14, seed + 3) + 3f * Noise(t, 41, seed + 4));
        }

        int tx = rng.Next(4, gap);
        while (tx < width)
        {
            int h = rng.Next(minH, maxH);
            int baseY = height - 4;
            bool dead = rng.NextDouble() < deadRatio;
            if (dead)
            {
                // Kuru ağaç: çıplak gövde, sarkık kırık dallar
                int trunk = h > maxH * 0.6f ? 3 : 2;
                for (int y = baseY - h; y <= baseY; y++)
                    for (int k = 0; k < trunk; k++) Plot(tx + k, y, 0.3f);
                for (int y = baseY - h + 5; y < baseY - h / 4; y += rng.Next(5, 10))
                {
                    for (int side = -1; side <= 1; side += 2)
                    {
                        if (rng.NextDouble() < 0.25) continue;
                        int len = 4 + (int)((y - (baseY - h)) / (float)h * h * 0.28f * (0.5 + rng.NextDouble() * 0.6));
                        for (int i = 1; i <= len; i++)
                        {
                            int px = side < 0 ? tx - i : tx + trunk - 1 + i;
                            int py = y + (int)(i * 0.45f);
                            Plot(px, py, 0.3f);
                            if (i > 2 && i % 3 == 0) Plot(px, py - 1, 0.95f); // Dalda kar
                        }
                    }
                }
            }
            else
            {
                // Köknar: aşağı doğru genişleyen dal katları, her katın sol üstünde kar
                int tiers = Math.Max(3, h / 14);
                int maxHalf = Math.Max(5, (int)(h * (0.2f + (float)rng.NextDouble() * 0.08f)));
                int trunkH = Math.Max(3, h / 9);
                for (int y = baseY - trunkH; y <= baseY; y++) { Plot(tx, y, 0.26f); Plot(tx + 1, y, 0.26f); }
                int crown = h - trunkH;
                for (int i = 0; i < crown; i++)
                {
                    float along = i / (float)crown;                       // 0 = tepe
                    float tierPos = along * tiers % 1f;                   // Kat içinde 0-1
                    int half = (int)(maxHalf * (0.12f + 0.88f * along) * (0.55f + 0.45f * tierPos));
                    int py = baseY - trunkH - crown + i;
                    for (int dx = -half; dx <= half + 1; dx++)
                    {
                        // Sol üstten gelen ışık: katların üst kenarı ve sol yarısı karlı
                        bool snowy = tierPos < 0.34f && dx < half * 0.35f || dx <= -half + 1 && tierPos < 0.7f;
                        float v = snowy ? (dx < 0 ? 1f : 0.82f) : (dx > half * 0.3f ? 0.26f : 0.36f);
                        Plot(tx + dx, py, v);
                    }
                }
                Plot(tx, baseY - h - 1, 0.36f); Plot(tx, baseY - h - 2, 0.36f); // Tepe sürgünü
            }
            tx += rng.Next(Math.Max(3, gap / 3), gap);
        }

        for (int x = 0; x < width; x++)
            for (int y = height - mound[x]; y < height; y++)
                data[y * width + x] = Gray(y - (height - mound[x]) < 2 ? 0.95f : 0.42f);

        return Create(gd, width, height, data);
    }

    // ------------------------------------------------------------------
    // Zemin
    // ------------------------------------------------------------------

    /// <summary>
    /// Ön plandaki karlı zemin (gündüz aydınlığında üretilir; gece tonu çizimde verilir): rüzgârın taradığı kar,
    /// balast yatağı, traversler, ayak ve kızak izleri, buz parıltıları.
    /// </summary>
    public static Texture2D CreateGround(GraphicsDevice gd, int width, int height, int railRow, int seed)
    {
        var data = new Color[width * height];
        var rng = new Random(seed);
        var snowTop = new Vector3(0.93f, 0.95f, 1f);
        var snowBottom = new Vector3(0.3f, 0.34f, 0.46f);
        var ballast = new Vector3(0.3f, 0.31f, 0.36f);
        var sleeper = new Vector3(0.19f, 0.16f, 0.15f);

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)width;
            int surface = 6 + (int)MathF.Round(2.5f * MathF.Sin(MathHelper.TwoPi * 4 * t) + 1.5f * MathF.Sin(MathHelper.TwoPi * 9 * t + 1.3f));
            for (int y = surface; y < height; y++)
            {
                float g = (y - surface) / (float)(height - surface);
                var c = Vector3.Lerp(snowTop, snowBottom, MathF.Pow(g, 0.8f));
                if (y - surface < 2) c = Vector3.Lerp(c, Vector3.One, 0.6f);
                // Sastrugi: rüzgârın açtığı uzun, yatık kar olukları
                float drift = Noise(t * 1f + y * 0.031f, 23, seed + y / 9);
                if (y > railRow + 14 && drift > 0.7f) c *= 0.9f;
                else if (y > railRow + 14 && drift < 0.16f) c = Vector3.Lerp(c, Vector3.One, 0.18f);
                c *= 1f + ((float)rng.NextDouble() * 0.06f - 0.03f);
                data[y * width + x] = new Color(Vector3.Clamp(c, Vector3.Zero, Vector3.One));
            }
        }

        // Balast yatağı: karın arasından görünen taşlar
        for (int y = railRow + 2; y < railRow + 11; y++)
            for (int x = 0; x < width; x++)
            {
                var stone = ballast * (0.8f + 0.5f * (float)rng.NextDouble());
                data[y * width + x] = new Color(Vector3.Lerp(stone, data[y * width + x].ToVector3(), rng.NextDouble() < 0.18 ? 0.8f : 0.15f));
            }

        // Traversler (32 px aralık; genişlik 32'nin katı olduğu için döşeme dikişsiz)
        for (int sx = 0; sx < width; sx += 32)
            for (int y = railRow + 1; y < railRow + 8; y++)
                for (int x = sx; x < sx + 14 && x < width; x++)
                    data[y * width + x] = new Color(sleeper * (y == railRow + 1 ? 1.5f : 1f));

        // Buz parıltıları
        for (int i = 0; i < 520; i++)
        {
            int x = rng.Next(width), y = rng.Next(railRow + 12, height);
            data[y * width + x] = Color.Lerp(data[y * width + x], Color.White, 0.8f);
        }
        return Create(gd, width, height, data);
    }
}
