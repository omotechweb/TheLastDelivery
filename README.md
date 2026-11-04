# The Last Delivery

Karlarla kaplı bir dünyada eski bir buharlı treni süren bir kuryenin hikâyesi.

Büyük Donma'dan bu yana yirmi yıl geçmiş. Geriye kalan kasabalar tek bir demiryolu hattının etrafına toplanmış, ısı paradan değerli. Arthur Hale de bu hatta yıllardır kurye treni süren, kimseyle pek konuşmayan bir adam. Bir gece eline mühürlü, garip bir şekilde sıcak bir paket veriyorlar ve iki bin kilometre ötedeki Sığınak-0'a götürmesini istiyorlar. Tek şart: açmayacak, soru sormayacak, durmayacak.

Oyun bu yolculuğu anlatıyor. Arthur'un neden geçmişinden kaçtığını, paketin içinde ne olduğunu ve yirmi yıldır bu hattı kimin dinlediğini yol boyunca yavaş yavaş öğreniyorsunuz.

## Nasıl bir oyun?

Bunu her şeyden önce bir hikâye oyunu olarak düşündüm. Treni sürmek, kazanı sıcak tutmak, yakıtı idare etmek, kurtlardan ve pusulardan kaçmak işin bir yarısı. Öbür yarısı ise insanlar: trene aldığınız yolcular, uğradığınız kasabalardaki tavernacılar, şerifler, tamirciler ve hepsinin size karşı ne hissettiği.

Kabaca şunlar var:

- **İki bölüm, toplam 80 bölüm.** Birincisi Sığınak-0'a giden yol. İkincisi, *Project AURA*, ilkinin bıraktığı yerden başlıyor ve kuzeye, dünyayı donduran makineye gidiyor. İlk bölümdeki kararlarınız ikinciye taşınıyor.
- **Her bölüm bir anlatım sahnesiyle açılıyor.** Kodla çizilmiş resimlerin üstünde sayfa sayfa akan kısa bir hikâye oluyor bu. İsteyen BACKSPACE'e basılı tutup geçebilir ama yazarken en çok emek verdiğim yerler bunlar.
- **İnsanlar ne yaptığınızı hatırlıyor.** Herkesin size karşı bir güveni var. Birini sürekli suçlar ya da tehdit ederseniz bir gece trenden kaçabilir, sizi arkadan vurabilir ya da yerinizi birilerine satabilir.
- **Kasabalarda binalara girebiliyorsunuz.** Tavernalar, karakollar, atölyeler, sığınaklar ve telsiz kuleleri var. İçlerinde konuşacak insanlar, okunacak belgeler ve dinlenecek eski ses kayıtları buluyorsunuz. Hepsi `L` tuşuyla açılan arşivde birikiyor.
- **Akıl sağlığı önemli.** Arthur yoruldukça göstergeler yalan söylemeye başlıyor, haritada olmayan istasyonlar beliriyor, telsizde tanıdık sesler çıkıyor.

Bunların yanında birkaç ek mod da var: sonsuz kurye, zamana karşı yarış ve tek canın olduğu zor bir mod.

## Kontroller

Sürüşte:

```
W / S       hızlan / yavaşla
A / D       makas değiştir
E           vanayı onar
1-5         eşya kullan
SHIFT       buhar
C           sobayı yak
G           yolculara yemek ver
F           kurtlara yem at
T           telsiz
K           kabin (yolcuları sorgula)
J           bir yolcuyla konuş
L           arşiv
N           mini harita
M           müzik kutusu
P           fotoğraf
ESC         duraklat
```

Kasabalarda A/D ile yürüyüp E ile etkileşime giriyorsunuz. Diyaloglarda seçenekleri 1-4 ile seçiyor, SPACE ile ilerliyorsunuz.

## Diller

Oyun Türkçe ve İngilizce olarak tam yazıldı. İspanyolca, Çince, Hintçe ve Arapça arayüz çevirileri de var, ama hikâyenin yeni kısımları bu dillerde henüz İngilizce görünüyor. Çevirmeye yardım etmek isteyen olursa çok sevinirim.

## Çalıştırmak

.NET 8 SDK yeterli. Oyun MonoGame ile yazıldı ve hiç harici grafik dosyası kullanmıyor; her şey kodla çiziliyor, o yüzden ayrı bir içerik derleme adımı da yok.

```bash
dotnet run
```

Tek dosyalık bir sürüm almak için:

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

Kurulum dosyası `installer/` klasöründeki Inno Setup betiğiyle hazırlanıyor. Kayıtlar `%LocalAppData%\TheLastDelivery\save.json` dosyasında duruyor.

## Kodun içinde gezinmek isteyenler için

Hikâyenin neredeyse tamamı veri olarak duruyor, kodun içine gömülü değil. Diyaloglar `Story/NarrativeData*.cs` dosyalarında düğümler olarak tanımlı; metinleri `Localization/` altındaki dil tablolarında `nv.` ile başlayan anahtarlarla duruyor. Bölüm açılışındaki anlatım sahneleri `Story/CinematicScript.E1.cs` ve `E2.cs` dosyalarında. Her satır bir sayfa ve İngilizcesiyle Türkçesi yan yana yazılı.

Birkaç yer daha:

- `Game1.cs`: ana oyun döngüsü
- `Story/DialogueTreeManager.cs`: güven sistemi ve sonuçları
- `Towns/`: kasabalar ve binaların içi
- `World/Train.cs`: trenin kendisi
- `Core/GameConfig.cs`: oyunun bütün denge ayarları

`ModKit/` klasöründe basit bir mod örneği var. Metinleri değiştirmek ya da yeni metin eklemek için oradan başlayabilirsiniz.

---

*In English:* The Last Delivery is a story-focused train game about a courier crossing a frozen world with a package he was told never to open. It's written in C# with MonoGame and draws every graphic in code. The full story is available in English and Turkish. Run it with `dotnet run`.
