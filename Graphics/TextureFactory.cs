using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLastDelivery.Graphics;

/// <summary>
/// Harici asset kullanmadan, piksel verisini doğrudan hesaplayarak Texture2D üreten fabrika.
/// Tüm renkler SpriteBatch'in varsayılan "premultiplied alpha" karışımına uygun üretilir.
/// </summary>
public static class TextureFactory
{
    /// <summary>1x1 beyaz piksel. Renklendirilip esnetilerek her türlü dikdörtgen çizilir.</summary>
    public static Texture2D CreatePixel(GraphicsDevice gd)
    {
        var texture = new Texture2D(gd, 1, 1);
        texture.SetData(new[] { Color.White });
        return texture;
    }

    /// <summary>Kenarları yumuşatılmış dolu daire (duman, ay, kar).</summary>
    public static Texture2D CreateCircle(GraphicsDevice gd, int radius)
    {
        int size = radius * 2;
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - radius + 0.5f;
                float dy = y - radius + 0.5f;
                float distance = MathF.Sqrt(dx * dx + dy * dy);
                float alpha = MathHelper.Clamp(radius - distance, 0f, 1f);
                data[y * size + x] = Color.White * alpha;
            }
        }
        return Create(gd, size, size, data);
    }

    /// <summary>Merkezden dışa doğru sönen ışık halesi (fener, lamba, ay ışığı).</summary>
    public static Texture2D CreateGlow(GraphicsDevice gd, int radius)
    {
        int size = radius * 2;
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - radius + 0.5f) / radius;
                float dy = (y - radius + 0.5f) / radius;
                float t = MathHelper.Clamp(1f - MathF.Sqrt(dx * dx + dy * dy), 0f, 1f);
                data[y * size + x] = Color.White * (t * t);
            }
        }
        return Create(gd, size, size, data);
    }

    /// <summary>1 piksel genişliğinde dikey renk geçişi; ekran boyunca esnetilerek gökyüzü olur.</summary>
    public static Texture2D CreateVerticalGradient(GraphicsDevice gd, int height, Color top, Color bottom)
    {
        var data = new Color[height];
        for (int y = 0; y < height; y++)
            data[y] = Color.Lerp(top, bottom, (float)y / (height - 1));
        return Create(gd, 1, height, data);
    }

    /// <summary>Ekran kenarlarını karartan vinyet (soğuk, kapalı bir atmosfer hissi).</summary>
    public static Texture2D CreateVignette(GraphicsDevice gd, int width, int height)
    {
        var data = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (x / (float)width - 0.5f) * 2f;
                float ny = (y / (float)height - 0.5f) * 2f;
                float d = MathF.Sqrt(nx * nx * 0.8f + ny * ny);
                float a = MathHelper.Clamp((d - 0.55f) / 0.65f, 0f, 1f);
                data[y * width + x] = Color.White * (a * a);
            }
        }
        return Create(gd, width, height, data);
    }

    /// <summary>Lokomotif farının sağa doğru açılan ışık konisi. Tepe noktası dokunun sol-ortasıdır.</summary>
    public static Texture2D CreateLightCone(GraphicsDevice gd, int width, int height)
    {
        var data = new Color[width * height];
        float half = height / 2f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float spread = x * 0.42f + 3f;              // Koni uzaklaştıkça genişler
                float across = MathF.Abs(y - half) / spread;  // 0 = merkez çizgisi, 1 = kenar
                if (across >= 1f) continue;

                float along = 1f - x / (float)width;          // Uzaklaştıkça söner
                float a = MathF.Pow(1f - across, 1.5f) * MathF.Pow(along, 1.3f);
                data[y * width + x] = Color.White * a;
            }
        }
        return Create(gd, width, height, data);
    }

    /// <summary>Dönme hareketi görülebilsin diye parmaklıklı (jantlı) tren tekerleği.</summary>
    public static Texture2D CreateWheel(GraphicsDevice gd, int radius, Color rim, Color spoke, int spokeCount)
    {
        int size = radius * 2;
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - radius + 0.5f;
                float dy = y - radius + 0.5f;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (d > radius) continue;

                Color c = Color.Transparent;
                if (d > radius - 3.5f) c = rim;           // Dış çember (bandaj)
                else if (d < 3.5f) c = rim;               // Göbek
                else
                {
                    for (int i = 0; i < spokeCount; i++)  // Jant telleri
                    {
                        float angle = MathHelper.Pi * i / spokeCount;
                        float lineDistance = MathF.Abs(dx * MathF.Sin(angle) - dy * MathF.Cos(angle));
                        if (lineDistance < 1.1f) { c = spoke; break; }
                    }
                }
                data[y * size + x] = c;
            }
        }
        return Create(gd, size, size, data);
    }

    /// <summary>
    /// Uzak dağ silüetleri. Tamsayı frekanslı sinüsler kullanıldığı için doku kenardan kenara
    /// kusursuz döşenir (sonsuz parallax için şart).
    /// </summary>
    public static Texture2D CreateMountainLayer(GraphicsDevice gd, int width, int height,
        Color color, Color rimColor, Color fogColor)
    {
        var data = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / width;
            float elevation = 0.55f * Ridge(t, 2, 0.3f) + 0.30f * Ridge(t, 5, 1.7f) + 0.15f * Ridge(t, 13, 4.1f);
            int top = height - (int)(height * (0.25f + 0.7f * elevation));

            for (int y = Math.Max(0, top); y < height; y++)
            {
                Color c = color;
                int depth = y - top;
                if (depth < 2) c = rimColor;                                   // Ay ışığı kenar parlaması
                else if (depth < 14 && elevation > 0.6f) c = Color.Lerp(color, rimColor, 0.25f); // Zirve karı

                float fog = MathHelper.Clamp((y - height * 0.55f) / (height * 0.45f), 0f, 1f) * 0.5f;
                data[y * width + x] = Color.Lerp(c, fogColor, fog);
            }
        }
        return Create(gd, width, height, data);
    }

    /// <summary>Kurumuş, dalları kırık çam ağacı silüetleri (orta katman).</summary>
    public static Texture2D CreateDeadPineLayer(GraphicsDevice gd, int width, int height, Color color, int seed)
    {
        var data = new Color[width * height];
        var rng = new Random(seed);

        // Doku sınırını aşan dallar karşı kenara sarılır, böylece döşeme dikişsiz olur.
        void Plot(int x, int y)
        {
            if (y < 0 || y >= height) return;
            x = ((x % width) + width) % width;
            data[y * width + x] = color;
        }

        int treeX = rng.Next(10, 40);
        while (treeX < width)
        {
            int treeHeight = rng.Next(height / 3, height - 4);
            int trunkWidth = treeHeight > height * 0.6f ? 3 : 2;
            int baseY = height - 1;
            int topY = baseY - treeHeight;

            // Gövde
            for (int y = topY; y <= baseY; y++)
                for (int t = 0; t < trunkWidth; t++)
                    Plot(treeX + t, y);

            // Dallar: aşağı doğru sarkık, bazıları kırık
            for (int y = topY + 4; y < baseY - treeHeight / 5; y += rng.Next(4, 8))
            {
                float heightRatio = (float)(baseY - y) / treeHeight; // 0 = dip, 1 = tepe
                int maxLength = (int)((1f - heightRatio) * treeHeight * 0.3f) + 3;

                for (int side = -1; side <= 1; side += 2)
                {
                    if (rng.NextDouble() < 0.22) continue; // Kırılmış dal

                    int length = (int)(maxLength * (0.5 + rng.NextDouble() * 0.6));
                    int endX = treeX, endY = y;
                    for (int i = 1; i <= length; i++)
                    {
                        endX = side < 0 ? treeX - i : treeX + trunkWidth - 1 + i;
                        endY = y + (int)(i * 0.45f);
                        Plot(endX, endY);
                        if (i < length * 0.4f) Plot(endX, endY + 1); // Dal dibi daha kalın
                    }

                    // Uçta yukarı kıvrılan küçük bir sürgün
                    if (length > 6 && rng.NextDouble() < 0.5)
                        for (int j = 1; j <= 3; j++)
                            Plot(endX + side * j, endY - j);
                }
            }

            treeX += rng.Next(18, 60);
        }

        // Ağaç diplerindeki kar yığıntısı
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / width;
            int moundHeight = 8 + (int)(3f * MathF.Sin(MathHelper.TwoPi * 6 * t) + 2f * MathF.Sin(MathHelper.TwoPi * 15 * t));
            for (int y = height - moundHeight; y < height; y++)
                data[y * width + x] = color;
        }

        return Create(gd, width, height, data);
    }

    /// <summary>
    /// Ön plandaki karlı zemin: dalgalı kar yüzeyi, balast, düzenli aralıklı traversler
    /// ve parıldayan buz kristalleri. Traversler kaydıkça hız hissi en çok buradan gelir.
    /// </summary>
    public static Texture2D CreateSnowGround(GraphicsDevice gd, int width, int height, int railRow, int seed)
    {
        var data = new Color[width * height];
        var rng = new Random(seed);

        var snowTop = new Color(185, 195, 215);
        var snowBottom = new Color(40, 46, 66);
        var ballast = new Color(52, 54, 64);
        var sleeper = new Color(34, 29, 27);

        // Kar yüzeyi
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / width;
            int surface = 6 + (int)MathF.Round(2.5f * MathF.Sin(MathHelper.TwoPi * 4 * t)
                                             + 1.5f * MathF.Sin(MathHelper.TwoPi * 9 * t + 1.3f));

            for (int y = surface; y < height; y++)
            {
                float g = (float)(y - surface) / (height - surface);
                Color c = Color.Lerp(snowTop, snowBottom, MathF.Pow(g, 0.7f));
                if (y - surface < 2) c = Color.Lerp(c, Color.White, 0.5f);

                float noise = 1f + ((float)rng.NextDouble() * 0.08f - 0.04f);
                data[y * width + x] = new Color(c.ToVector3() * noise);
            }
        }

        // Balast yatağı
        for (int y = railRow + 2; y < railRow + 11; y++)
            for (int x = 0; x < width; x++)
                data[y * width + x] = Color.Lerp(ballast, data[y * width + x], 0.2f);

        // Traversler (32 px aralık; 1024 / 32 tam bölündüğü için döşeme dikişsiz)
        for (int sx = 0; sx < width; sx += 32)
            for (int y = railRow + 1; y < railRow + 8; y++)
                for (int x = sx; x < sx + 14 && x < width; x++)
                    data[y * width + x] = sleeper;

        // Buz parıltıları
        for (int i = 0; i < 450; i++)
        {
            int x = rng.Next(width);
            int y = rng.Next(railRow + 12, height);
            data[y * width + x] = Color.Lerp(data[y * width + x], Color.White, 0.7f);
        }

        return Create(gd, width, height, data);
    }

    // ------------------------------------------------------------------

    private static float Ridge(float t, int frequency, float phase)
        => 1f - MathF.Abs(MathF.Sin(MathHelper.Pi * frequency * t + phase));

    /// <summary>
    /// Tipide ekran kenarlarına yapışan buz: kenarda yoğun, içe doğru seyrelen, kristal benekli ve
    /// eğreltiotu gibi dallanan çizgili bir buğu. Ortası tamamen saydamdır.
    /// </summary>
    public static Texture2D CreateFrostEdge(GraphicsDevice gd, int width, int height, int seed)
    {
        var data = new Color[width * height];
        float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) / (float)uint.MaxValue;
            }
        }

        // Kaba ölçekli yumuşak gürültü (4 köşe arasında ara değer)
        float Smooth(float x, float y, int cell)
        {
            int cx = (int)(x / cell), cy = (int)(y / cell);
            float fx = x / cell - cx, fy = y / cell - cy;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float top = MathHelper.Lerp(Hash(cx, cy), Hash(cx + 1, cy), fx);
            float bottom = MathHelper.Lerp(Hash(cx, cy + 1), Hash(cx + 1, cy + 1), fx);
            return MathHelper.Lerp(top, bottom, fy);
        }

        float minSide = MathF.Min(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float edge = MathF.Min(MathF.Min(x, width - 1 - x), MathF.Min(y, height - 1 - y)) / (minSide * 0.5f);
                // Kenardan içeri sokulan düzensiz buz sınırı
                float reach = 0.22f + 0.2f * Smooth(x, y, 24) + 0.08f * Smooth(x, y, 7);
                float t = MathHelper.Clamp(1f - edge / reach, 0f, 1f);
                if (t <= 0f) continue;

                // Kristal benekleri ve dallanan çizgiler
                float speck = Hash(x, y) > 0.82f ? 0.35f : 0f;
                float fern = MathF.Abs(MathF.Sin((x + y * 0.6f) * 0.35f + Smooth(x, y, 12) * 9f)) > 0.93f ? 0.4f : 0f;
                float a = MathHelper.Clamp(t * t * (0.55f + 0.3f * Smooth(x, y, 5)) + (speck + fern) * t, 0f, 1f);
                data[y * width + x] = Color.White * a;
            }
        }
        return Create(gd, width, height, data);
    }

    private static Texture2D Create(GraphicsDevice gd, int width, int height, Color[] data)
    {
        var texture = new Texture2D(gd, width, height);
        texture.SetData(data);
        return texture;
    }
}
