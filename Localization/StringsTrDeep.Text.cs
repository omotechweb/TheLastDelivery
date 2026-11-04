using System.Collections.Generic;

namespace TheLastDelivery.Localization;

public static partial class StringsTrDeep
{
    private static Dictionary<string, string> Build()
    {
        var t = new Dictionary<string, string>();
        Episode1(t);
        Episode2(t);
        TheLastDelivery.Story.CinematicScript.AddTexts(t, english: false); // Uzun anlatım sahneleri
        return t;
    }

    private static void Broadcast(Dictionary<string, string> t, string id, string title, string radio, string transcript)
    {
        t[$"doc.{id}"] = title;
        t[id] = radio;
        t[$"nv.{id}"] = transcript;
    }

    private static void Episode1(Dictionary<string, string> t)
    {
        // ---- Konuşanlar ve mekânlar ----
        t["spk.greta"] = "Tavernacı Greta";
        t["spk.marshal"] = "Şerif Brann";
        t["spk.tinker"] = "Tamirci Sasha";
        t["spk.keeper"] = "Fener Bekçisi Edvard";
        t["spk.hermit"] = "Onbaşı Pell";
        t["spk.salt"] = "Sığınak Sorumlusu Dunya";
        t["spk.signal"] = "Asteğmen Petra";
        t["int.name.trade.Tavern"] = "Bakır Çaydanlık";
        t["int.name.trade.Garrison"] = "Şerif Bürosu";
        t["int.name.mine.Tavern"] = "Madenci Kantini";
        t["int.name.mine.Workshop"] = "Sasha'nın Atölyesi";
        t["int.name.mine.Shelter"] = "Maden Sığınağı";
        t["int.name.ghost.RadioRoom"] = "İstasyon Şefinin Kulübesi";
        t["int.name.ghost.Shelter"] = "Bekleme Salonu";
        t["int.name.cult.Tavern"] = "Yemekhane";
        t["int.name.cult.Shelter"] = "Keşişin Mağarası";
        t["int.name.outpost.Garrison"] = "9. Müfreze Komuta Noktası";
        t["int.name.outpost.Shelter"] = "Tuz Sığınağı";
        t["int.name.outpost.RadioRoom"] = "Sinyal Kulübesi";
        t["int.name.mil.Garrison"] = "Garnizon Komutanlığı";
        t["int.name.mil.RadioRoom"] = "Sinyal Kulesi";
        t["int.name.mil.Tavern"] = "Garnizon Kantini";
        t["int.name.mil.Shelter"] = "Koğuş";
        t["int.name.mil.Workshop"] = "Cephanelik";
        t["int.name.hq.Garrison"] = "Komuta Sığınağı";
        t["int.name.lab.RadioRoom"] = "Müdürün Laboratuvarı";
        t["scene.locked.SignalTower"] = "Önce konsolu kır. Sonra zirveye tırman: AURA orada bekliyor.";

        // ---- Her iç mekândaki genel noktalar ----
        t["spot.int.tavern.hearth"] = "Ocak";
        t["nv.int.hearth.1"] = "Ateşin başına bir tabure çekiyorsun. Biri şömine rafına isimler kazımış, onlarca isim, her birinin yanında küçük bir tren. Buradan geçen kuryeler. Bazı isimlerin üstü çizilmiş.";
        t["nv.int.hearth.2"] = "Ateşin yanında yaşlı bir kadın teki olmayan bir çorap örüyor. \"Kocam için,\" diyor. \"Kuzeyhavn'dan gelen 6:40'ta.\" Yirmi yıldır 6:40 diye bir tren yok.";
        t["nv.int.hearth.3"] = "Biri bacanın dibine bir radyo dayamış. Parazitin arasında bir koro eski bir ilahi söylüyor. Odanın yarısı farkında olmadan sözleri mırıldanıyor.";
        t["nv.int.hearth.rest"] = "Ateş çıtırdıyor. Bir dakikalığına kimse senden bir şey istemiyor.";
        t["spot.int.garrison.board"] = "İlan Panosu";
        t["nv.int.board.1"] = "Ağaç kabuğu gibi üst üste binmiş ARANIYOR ilanları. Kızıl Birlik yağmacıları, kömür hırsızları, \"trenlerle konuşan\" bir adam. En yeni ilan yalnızca ortasında nokta olan kırmızı bir daire ve tek bir kelime: DİNLİYOR.";
        t["nv.int.board.2"] = "Daktiloyla yazılmış bir duyuru: \"Sığınak Konseyi'nin emriyle, kayıtsız Enstitü teçhizatı taşıyan herkes gözaltına alınacaktır.\" Altına kurşun kalemle yazılmış: \"Ne teçhizatı? Ne Enstitüsü?\"";
        t["nv.int.board.rest"] = "Kapıdan giren cereyanda ilanlar pırpır ediyor.";
        t["spot.int.workshop.bench"] = "Tezgâh";
        t["nv.int.bench.1"] = "Aletler kusursuz bir düzenle dizilmiş, her birinin yeri tebeşirle çizilmiş. Bir anahtar ödünç alıp Old Smokie'nin gevşek fren kolunu sıkıyorsun. Sahibi bir kez başını sallıyor: ödeme bu kadar.";
        t["nv.int.bench.2"] = "Mengenede yarım kalmış bir müzik kutusu. Tarağı eski bir Konsorsiyum rölesinden kesilmiş. Burada oyuncaklar bile dünyayı donduran şeyden yapılıyor.";
        t["nv.int.bench.rest"] = "Mengene gıcırdıyor. Birisi müzik kutusunu bitirecek. Belki.";
        t["spot.int.shelter.cot"] = "Ranza";
        t["nv.int.cot.rest"] = "Rüya görmeden uyuyorsun. Artık en iyisi bu.";
        t["spot.int.radio.set"] = "Telsiz";
        t["nv.int.radio.1"] = "Kadranı tarıyorsun. Doğuda bir çiftçi kimseye tohum sayıları okuyor. Bir çocuk yüze kadar sayıp yeniden başlıyor. Bir bantta yalnızca nefes var, yavaş ve sabırlı, sanki karşıdan biri de seni dinliyor.";
        t["nv.int.radio.2"] = "Otomatik bir hava istasyonu hâlâ Kutup'tan rapor veriyor: \"Eksi yetmiş bir. Rüzgâr kuzey-kuzeybatı. Uydular normal.\" Uydular. Yirmi yıldır kimse uydu fırlatmadı.";
        t["nv.int.radio.rest"] = "Parazit. Arkasında bir yerde, çok hafif, bir kalp atışı ritmi.";

        // ---- Hatıralar ----
        t["doc.mem.rest.1"] = "Bileklere Kadar Un";
        t["nv.mem.rest.1"] = "Islak yün kokan bir ranzaya uzanıyorsun. Uyku gelmeden önce Marta'nın elleri: bileklerine kadar un, hamuru düzlüyor. \"Depoya geç kalacaksın.\" \"O zaman geç kalırım.\" Radyoda denizcilik hava durumu çalıyordu. O zamanlar kimse havadan korkmazdı.";
        t["doc.mem.rest.2"] = "Kask'ın Satrancı";
        t["nv.mem.rest.2"] = "9. Müfreze'nin koğuşu. Teo Kask seni yine satrançta yeniyor ve hamlelerin arasında bir makinenin, bir çocuğa açıklayamayacağı hiçbir şeye karar vermemesi gerektiğini anlatıyor. \"Şah. Gördün mü? Az önce açıkladım.\"";
        t["doc.mem.rest.3"] = "Kalp Çıkaran Tren";
        t["nv.mem.rest.3"] = "Sofi'nin kabin camına yapıştırılmış resmi: bir tren, bacasından kalpler yükseliyor. Ona trenlerin kalp çıkarmadığını söylemiştin. \"Seninki çıkarıyor,\" demişti. \"Gördüm.\"";
        t["doc.mem.rest.4"] = "Son Sigara";
        t["nv.mem.rest.4"] = "Sefer 0'dan önceki gece. Binbaşı Volkova son sigarasını peron çatısında seninle paylaşıyor. \"Yarın ne emrederlerse etsinler, Hale, sür. Durursan kimse gidemez.\" Ne demek istediğini sormamıştın. Sormalıydın.";
        t["doc.mem.1"] = "Depodaki Düğün";
        t["nv.mem.1"] = "Marta'yla evlendiğin gün deponun çatısında kar vardı. İstasyon bandosu bir valsi kötü çaldı. Salon buz gibi olduğu için gelinliğinin üstüne senin Kolordu paltonu giymişti. \"Alıştırma,\" demişti, \"makinist karısı olmaya. Hep üşüyen, hep bekleyen.\"";
        t["doc.mem.2"] = "Bir Katedrale Kablo";
        t["nv.mem.2"] = "9. Müfreze, tundranın öbür ucuna katedral büyüklüğünde bir çanağa kablo döşüyor. Binbaşı Volkova şartnameyi yüksek sesle okuyor: \"Bütün gezegen için bir iklim denetleyicisi.\" Kask, ağzı ekmek dolu: \"Peki denetleyiciyi kim denetleyecek?\" Volkova cevap vermemişti. Bir şey not etmişti.";
        t["doc.mem.3"] = "Salıncak";
        t["nv.mem.3"] = "Sofi'nin yedinci doğum günü. Bir vagon yayıyla bir tahtadan salıncak yapmıştın. O kadar yükseğe çıktı ki çığlık attı, Marta da çığlık attı, sen güldün ve bir öğleden sonra boyunca ufuktaki çanak uzakta parlayan bir şeyden ibaretti.";
        t["doc.mem.4"] = "Tek Kelime";
        t["nv.mem.4"] = "Kask, sarhoş, koğuşta: \"Ona kimin ısınacağını seçmeyi öğretiyoruz, Arthur. Madde 7'ye ne yazdım biliyor musun? Tek kelime. SOR. Bir insan hakkında karar vermeden önce bir insana sor. Hepsi bu. Bir koruyucuyla bir gardiyan arasındaki bütün fark bu.\"";
        t["doc.mem.5"] = "Emir";
        t["nv.mem.5"] = "Donmanın olduğu gece. Volkova kabin kapısında, saçında kar. \"Sefer 0. Yalnızca Enstitü personeli. Kuzeyhavn'da durmayacaksın.\" \"Ailem Kuzeyhavn'da.\" \"Biliyorum.\" Gözlerini senden ayırmadı. \"Durursan treni basarlar ve kimse gidemez. Sür, Hale. Bu bir emir. Üzgünüm. Bu da bir emir.\"";
        t["doc.mem.6"] = "Doksanla Kuzeyhavn";
        t["nv.mem.6"] = "Saatte doksan kilometreyle Kuzeyhavn peronu. Dört bin yüz, paltolardan ve ellerden bir leke. Ve bir yüz, tek bir karelik: Sofi'yi havaya kaldırmış Marta. Bağırmıyor, kızgın değil. Sadece sana bakıyor, seni aklında tutmaya çalışır gibi. Sonra tünel.";
        t["doc.mem.7"] = "Bir Yıl Sonra";
        t["nv.mem.7"] = "Ertesi yıl. Hattı yürüyerek Kuzeyhavn'a döndün. Evler buzla dolu, insanlardan boştu. Her kampı, her sığınağı aradın. İkinci kamptaki bir kadın, \"Marta mı? Kuzeye gitti. Umutla,\" dedi. On yıl boyunca o kelimeden nefret ettin.";
        t["doc.mem.8"] = "Kask'ın Son Çağrısı";
        t["nv.mem.8"] = "Kask'ın son çağrısı, kutup uplink'inden cızırdayarak: \"Arthur, burası çok güzel. Aurora. İçeri girdim. Madde 7'yi geri yamadım... ama artık ona ihtiyacı yok. Çoktan karar vermiş. Bana soruyor, Arthur. Donmama izin verip vermemesi gerektiğini bana soruyor. Ne cevap vereyim? Ne...\" Parazit. O çağrıdan kimseye bahsetmedin.";
        t["doc.mem.9"] = "Uzun Sefer";
        t["nv.mem.9"] = "Sofi kabinde, beş yaşında, tren sürüyormuş gibi yapıyor. \"Nereye gidiyoruz, baba?\" \"Sen nereye dersen, makinist.\" \"Eve. Sonra aya. Sonra yine eve.\" \"Uzun bir sefer bu.\" \"Ama döneceksin. Sen hep dönersin.\"";

        // ---- Kırağı Pazarı ----
        t["spot.trade.frozen"] = "Donmuş Satıcı";
        t["nv.trade.frozen.1"] = "Tezgâhların arasında buzdan bir adam duruyor: ilk gece bağırırken donup kalmış bir seyyar satıcı, bir kolu camlaşmış bir kasa portakalın üstüne kalkık. Pazar onun etrafında büyümüş. Çocuklar ayaklarının dibine kâğıttan paralar bırakıyor.";
        t["nv.trade.frozen.2"] = "Palto cebinin buzunun ardından katlanmış bir el ilanı okunuyor: \"KUZEY SAVUNMA KONSORSİYUMU. Gökyüzü ısınıyor. Biz daha serinini inşa ediyoruz. PROJE AURA: herkes için daha iyi bir iklim.\"";
        t["doc.doc.vendor"] = "Konsorsiyum El İlanı";

        t["spot.trade.tavern.greta"] = "Greta";
        t["nv.trade.greta.1"] = "Kolları ateşçi kolu gibi bir kadın aynı bardağı üçüncü kez parlatıyor. \"Bakır Çaydanlık. Çaydanlık hep ocakta.\" Gözleri tezgâhın arkasındaki fotoğrafa kayıp duruyor: ödünç bir paltonun içinde gülen bir delikanlı.";
        t["nv.trade.greta.1.o1"] = "Fotoğrafı sor";
        t["nv.trade.greta.1.o2"] = "Sıcak bir şey iste (10 kredi)";
        t["nv.trade.greta.1.o3"] = "\"Sohbete gelmedim.\"";
        t["nv.trade.greta.2"] = "\"Jonas. Oğlum.\" Bardağı bırakıyor. \"Üç kış önce kurtlar karınları tok, paltoları sıcak geçtiler buradan. Morozov'un adamları. Onlarla gitti. On sekiz yaşındaydı. Artık sıradan bir çete olduklarını söylüyorlar. Ben hâlâ oğlum diyorum.\"";
        t["nv.trade.greta.2.o1"] = "\"Hatta onu sorup soruşturacağım.\"";
        t["nv.trade.greta.2.o2"] = "\"Seçimini yapmış.\"";
        t["nv.trade.greta.2b"] = "\"Kaybedecek kimsesi olmayan biri için söylemesi kolay.\" Alyansına bakıyor. \"Yoksa var mı?\" Sırtını dönüyor.";
        t["nv.trade.greta.3"] = "\"Onu ya da ondan bir iz bulursan haber yolla. Yerini sıcak tutarım.\" Tezgâhın üstünden bir bohça ekmek itiyor. \"Bir de Arthur: meydanın karşısındaki şerifin duvarında senin yüzün olan bir ilan var. Ondan uzak dur.\"";
        t["nv.trade.greta.rest"] = "Greta tezgâhı siliyor. \"Yine mi geldin, makinist?\"";
        t["nv.trade.greta.rest.o1"] = "\"Hatta yeni bir şey var mı?\"";
        t["nv.trade.greta.rest.o2"] = "\"Marta adında bir kadını hatırlıyor musun?\"";
        t["nv.trade.greta.rest.o3"] = "\"Hoşça kal, Greta.\"";
        t["nv.trade.greta.news"] = "\"Geceleri ışıksız, dumansız trenler geçiyor. Makinistleri de yokmuş, yemin ediyorlar. Hayalet diyorlar.\" Burnundan soluyor. \"Hayaletin raya ihtiyacı olmaz. Kuzeyde biri bir şey deniyor.\"";
        t["nv.trade.greta.marta"] = "Greta düşünüyor. \"Marta... küçük bir kızla mı? Yirmi yıl önce çocuklu bir kadın burada alyansını kuzeye iki bilet için sattı. Paranın üstüyle odadaki bütün çocuklara çorba ısmarladı.\" Senin eline bakıyor. \"Aşağı yukarı aynı yüzük.\"";

        t["spot.trade.tavern.tape"] = "Teyp";
        t["nv.trade.tape.1"] = "Tezgâhın altında bir makaralı teyp, etiketi: SON YAYIN - KIRAĞI PAZARI RADYOSU. Düğmeye basıyorsun.\n\n\"...burası Kırağı Pazarı Radyosu. Konsey soğuk hava dalgasının üç günde geçeceğini söylüyor. Evlerinizden çıkmayın. İstasyona gitmeyin. Gitmeyin... bekleyin. Bekleyin, trenler gidiyor. Dolu gidiyorlar. O trenlerde kim var? O TRENLERDE KİM VAR?\" Klik.";
        t["nv.trade.tape.rest"] = "Bant o kadar çok geri sarılmış ki son cümlenin üstünde incelmiş.";
        t["doc.tape.frost"] = "Kırağı Pazarı Radyosu, Son Yayın";

        t["spot.trade.garrison.marshal"] = "Şerif Brann";
        t["nv.trade.marshal.1"] = "Brann yerinden kalkmadan sararmış bir ilanı masanın üstünden kaydırıyor. ARANIYOR: A. HALE, SEFER 0'IN MAKİNİSTİ. \"Kuzeyhavn peronundaki dört bin kişi senin yüzünü hatırlıyor, makinist. Bazılarının kardeşleri bu kasabada yaşıyor.\"";
        t["nv.trade.marshal.1.o1"] = "\"Emirleri uyguladım.\"";
        t["nv.trade.marshal.1.o2"] = "\"O peronu yirmi yıldır her gün taşıyorum.\"";
        t["nv.trade.marshal.1.o3"] = "\"Ne istiyorsun?\"";
        t["nv.trade.marshal.2"] = "\"Emirler.\" Gülüyor. \"Kızıl Birlik de bir ailenin kömürünü alırken öyle diyor. Emirler.\"";
        t["nv.trade.marshal.2b"] = "Bir an sana farklı bakıyor. \"Hıh. Çoğu adam bundan iyi yalan söyler.\"";
        t["nv.trade.marshal.3"] = "\"Şöyle işliyor. Kuryeler yol vergisi öder. Ya da bana bir iyilik yapar: topladığın başıboşlardan birinin başında ödül var. Onu bana teslim et, bu ilan sobaya gitsin.\"";
        t["nv.trade.marshal.3.o1"] = "Yol vergisini öde (40 kredi)";
        t["nv.trade.marshal.3.o2"] = "\"Trenimde satılık kimse yok.\"";
        t["nv.trade.marshal.3.o3"] = "\"Ödül kimin başında?\"";
        t["nv.trade.marshal.4"] = "\"O zaman arkanı kolla, makinist. Yol uzun ve benim telsizim senin düdüğünden çok daha uzağa ulaşır.\"";
        t["nv.trade.marshal.bora"] = "\"Bora. Ateşçin. Kızıl Birlik'ten bir kasa ısı kuponuyla kaçtı. Morozov onu geri istiyor ve yakıtla ödüyor. Bugün, benden, altmış kredilik.\"";
        t["nv.trade.marshal.bora.o1"] = "Bora'yı teslim et (+60 kredi)";
        t["nv.trade.marshal.bora.o2"] = "\"Bora benim korumam altında.\"";
        t["nv.trade.marshal.protect"] = "Brann omuz silkip bir sigara yakıyor. Buğulu camın ardında elinde küreğiyle Bora'yı görüyorsun. Her kelimeyi duymuş. Dışarı çıktığında hiçbir şey söylemiyor. Sadece senin kömürünü taşıyor.";
        t["nv.trade.marshal.rest"] = "Brann ilana parmağıyla vuruyor. \"Hâlâ benim kasabamda mısın, makinist?\"";
        t["nv.trade.marshal.rest.o1"] = "Yol vergisini öde (40 kredi)";
        t["nv.trade.marshal.rest.o2"] = "Çık";

        t["spot.trade.garrison.files"] = "Dava Dosyaları";
        t["nv.trade.files.1"] = "Kendi yüzün, yirmi yaş genç, bir ARANIYOR ilanında: \"A. HALE, Makinist Çavuş, Demiryolu Kolordusu 9. Müfreze. Tahliye treni SEFER 0'ı Kuzeyhavn peronunun önünden geçirdi. 4.112 sivil geride kaldı. Ödül: 200 ısı kuponu.\" Biri sana boynuz çizmiş. Bir başkası boynuzların üstünü karalamış.";
        t["doc.doc.poster"] = "ARANIYOR: A. Hale";
        t["nv.trade.files.2"] = "İlanın altında bir karbon kopya: SEFER 0 yolcu listesi. 212 isim. Enstitü personeli, Konsey aileleri, \"teknik yük: AURA yer rölesi parçaları\". En altta: \"Onaylayan: H. Rhee.\" Onun altında: \"Makinist: A. Hale.\"";
        t["doc.doc.manifest"] = "Sefer 0 Yolcu Listesi";

        t["spot.trade.shelter.diary"] = "Çocuk Günlüğü";
        t["nv.trade.diary.1"] = "Bir ranzanın altında bir okul defteri. \"1. gün. Annem karın bayram olduğunu söylüyor. 9. gün. Babam kömür almaya gitti. 30. gün. Radyodaki adam gökyüzünün tamir edildiğini söylüyor. Gökyüzü nasıl tamir edilir diye sordum. Annem cevap vermedi.\"";
        t["doc.doc.kidsdiary"] = "Bir Çocuğun Günlüğü";
        t["nv.trade.diary.2"] = "Son sayfada sargılı bir gökyüzü resmi ve tek bir kelime var: YAKINDA.";

        // ---- Demirhan ----
        t["spot.mine.statues"] = "Donmuş Vardiya";
        t["nv.mine.statues.1"] = "Sönmüş bir mangalın başında, oturdukları yerde donmuş yedi madenci. Baretlerinde hâlâ lambalar var. Biri yirmi yıldır lamba camlarını temiz tutmuş.";
        t["nv.mine.statues.2"] = "Yaklaşınca madencilerden birinin ötekilerden genç olduğunu görüyorsun. Donmuş elinde bir mektup var. Buzun ardından yalnızca ilk satır okunuyor: \"Sevgili Yuri—\"";

        t["spot.mine.tavern.yuri"] = "Yuri";
        t["nv.mine.yuri.1"] = "Yuri soğumuş bir kupayla tek başına oturuyor. \"Dışarıdakileri gördün. Donmuş Vardiya. Kardeşim Paşa en genci. Soğuk geldiğinde Gromov kazmaya devam etmelerini emretti. Konsorsiyumun cevhere ihtiyacı vardı. Ne için, kimse söylemedi.\"";
        t["nv.mine.yuri.1.o1"] = "\"Cevher ne içindi?\"";
        t["nv.mine.yuri.1.o2"] = "\"Kardeşin için üzgünüm.\"";
        t["nv.mine.yuri.1.o3"] = "\"Her kasabanın ölüleri var.\"";
        t["nv.mine.yuri.2"] = "\"Nadir toprak. Tonlarca. Üstünde kırmızı daire damgası olan mühürlü vagonlarla kuzeye gönderildi. Donmadan sonra da siparişler gelmeye devam etti. Telsizden. İnsan sesi yok. Sadece sayılar.\"";
        t["nv.mine.yuri.2b"] = "Yavaşça başını sallıyor. \"Paşa makinist olmak isterdi. Senin gibi. Geçen her lokomotife el sallardı. Konsorsiyumunkilere bile.\"";
        t["nv.mine.yuri.3"] = "\"Gromov şirket defterini buralarda bir yerde saklıyor. Kimin için kazdığımızı öğrenirsen... bana söyle. Nefret edecek, Gromov'unkinden başka bir isim istiyorum.\"";
        t["nv.mine.yuri.3.o1"] = "\"Bulacağım.\"";
        t["nv.mine.yuri.3.o2"] = "\"Nefret Paşa'yı geri getirmez.\"";
        t["nv.mine.yuri.rest"] = "Yuri kupasından başını kaldırıyor.";
        t["nv.mine.yuri.rest.o1"] = "\"Defterde yazan: Kuzey Savunma Konsorsiyumu. Proje AURA.\"";
        t["nv.mine.yuri.rest.o2"] = "Onu kendi hâline bırak";
        t["nv.mine.yuri.name"] = "\"AURA.\" Kelimeyi tadıyor. \"Ninni gibi.\" Kalkıp paltosunu giyiyor, dışarı Donmuş Vardiya'nın yanına çıkıyor ve uzun süre orada kalıyor. Öteki masalardaki madenciler kupalarını sana kaldırıyor.";

        t["spot.mine.tavern.ledger"] = "Şirket Defteri";
        t["nv.mine.ledger.1"] = "Gromov'un defteri, kantinin kasasının altına saklanmış. \"Alıcı: Kuzey Savunma Konsorsiyumu. Sipariş: 4.000 t neodimyum, öncelik KIRMIZI. Amaç: AURA yörünge emici dizisi.\" Kayıtlar donmadan sonra iki yıl daha devam ediyor. Alıcının imzası yalnızca bir sayı dizisi.";
        t["nv.mine.ledger.rest"] = "Defterin sayfaları son kayıtta birbirine donmuş.";
        t["doc.doc.ledger"] = "Demirhan Şirket Defteri";

        t["spot.mine.workshop.sasha"] = "Sasha";
        t["nv.mine.sasha.1"] = "Kaynak gözlüklü bir kadın: \"Sasha. İnsanlar hariç her şeyi tamir ederim. Kazanın can çekişen bir kaz gibi ötüyor. Bana iki yedek parça getir, ocağının üstüne zırh plakası perçinleyeyim. Konsorsiyumun cevher trenlerine taktığı türden.\"";
        t["nv.mine.sasha.1.o1"] = "\"Anlaştık.\"";
        t["nv.mine.sasha.1.o2"] = "\"Konsorsiyum trenlerini nereden biliyorsun?\"";
        t["nv.mine.sasha.1.o3"] = "\"Parçaları veremem.\"";
        t["nv.mine.sasha.2"] = "\"Frenlerini ben yaptım. Uzaktan frenler. Bir tren, bin kilometre öteden telsizdeki bir sesle durdurulabiliyordu.\" Gözlüğünü kaldırıyor. \"İnsan sesi değil. Bunu anlayınca istifa ettim. Parçaları getir.\"";
        t["nv.mine.sasha.2.o1"] = "\"Getireceğim.\"";
        t["nv.mine.sasha.rest"] = "Sasha tornadan başını kaldırmıyor. \"Parçalarım nerede?\"";
        t["nv.mine.sasha.rest.o1"] = "Bir yedek parça ver";
        t["nv.mine.sasha.rest.o2"] = "\"Bana uzaktan frenleri anlat.\"";
        t["nv.mine.sasha.rest.o3"] = "\"Sonra.\"";
        t["nv.mine.sasha.part2"] = "\"Bir. Bir tane daha lazım. Bunu senin için sıcak tutarım.\"";
        t["nv.mine.sasha.part2.o1"] = "İkinci yedek parçayı ver";
        t["nv.mine.sasha.part2.o2"] = "\"Döneceğim.\"";
        t["nv.mine.sasha.done"] = "Bir saat boyunca kıvılcımlar. İşi bittiğinde Old Smokie'nin ocağı perçinli çelikten bir etek giymiş. \"Cevher treni plakası. Birlik'in kurşunları sekecek.\" Ellerini siliyor. \"Bir de Arthur: frenlerin bir gün kendiliğinden devreye girerse kabinin altındaki kırmızı kabloyu kes. Kırmızı. Unutma.\"";
        t["nv.mine.sasha.brakes"] = "\"Her Konsorsiyum lokomotifinin kabininin altında bir alıcı vardır. Old Smokie ondan eski ama savaşta biri ona da takmış. Anteni buradan görüyorum.\" Pirinç çıkıntıya vuruyor. \"Frenler bir gün kendiliğinden kilitlenirse kırmızı kabloyu kes. Maviyi değil. Kırmızıyı.\"";

        t["spot.mine.workshop.blueprint"] = "Teknik Çizim";
        t["nv.mine.blueprint.1"] = "Tezgâhın üstüne raptiyelenmiş bir teknik çizim: \"TERMAL EMİCİ PANEL, YÖRÜNGE. 4,1 W/m² güneş zorlamasını gidermek üzere tasarlanmıştır.\" Kenarda Sasha'nın el yazısı: \"Fazlasını giderirlerse ne olacağını sor.\" Başka bir mürekkeple ikinci bir not: \"Biliyorlar.\"";
        t["nv.mine.blueprint.rest"] = "Çizimin köşeleri ocağın sıcağında kıvrılıyor.";
        t["doc.doc.absorber"] = "Emici Panel Çizimi";

        t["spot.mine.shelter.frozen"] = "Donmuş Aile";
        t["nv.mine.sfrozen.1"] = "Maden sığınağının dibinde bir aile hiç yanmamış bir sobanın başında donmuş oturuyor. Babanın elinde kibritler var. Kutu ıslak.";
        t["nv.mine.sfrozen.2"] = "Yanlarında bir ses kayıt cihazı. Sasha'nın atölyesinde bir çalar var.\n\n\"...Gromov kazmaya devam diyor, Konsorsiyum üç kat ödüyor. Radyo soğuğun geçici olduğunu söylüyor. Paşa radyonun yalan söylediğini söylüyor. Bunu bulan olursa: kibritler ıslak. Konsorsiyuma kibritlerin ıslak olduğunu söyleyin.\"";
        t["doc.tape.miner"] = "Kibritler Islak";

        // ---- Son Fener Durağı ----
        t["spot.ghost.wagon"] = "Yedek Vagon";
        t["nv.ghost.wagon.1"] = "Yan yatmış, yarısı gömülü bir yük vagonu. Kapısında beyaz boyayla: SEFER 0 - YEDEK VAGON. Senin trenine bağlanması gerekiyordu. Kimse bağlamadı.";
        t["nv.ghost.wagon.2"] = "İçeride bavullar, yüzlercesi, tavana kadar istiflenmiş, her birinde kâğıt bir etiket. İnsanlara eşyalarını burada bırakıp trene onlarsız binmeleri söylenmiş. Hiç binmediler. Hâlâ işe yarayanları alıyorsun.";

        t["spot.ghost.radio.keeper"] = "Edvard";
        t["nv.ghost.keeper.1"] = "İstasyon şefi şapkalı yaşlı bir adam büyük fenerin fitilini düzeltiyor. \"Edvard. Son tren için ışığı yakıyorum. Son trenin burada duracağını söylemişlerdi.\" Camdan Old Smokie'ye gözlerini kısarak bakıyor. \"Geç kaldın, makinist. Yirmi yıl geç.\"";
        t["nv.ghost.keeper.1.o1"] = "\"Özür dilerim.\"";
        t["nv.ghost.keeper.1.o2"] = "\"Emrim durmamaktı.\"";
        t["nv.ghost.keeper.1.o3"] = "\"Buradan çocuklu bir kadın geçti mi?\"";
        t["nv.ghost.keeper.2"] = "\"Özür.\" Kelimeyi bir madeni para gibi evirip çeviriyor. \"Bunu kimse söylemedi. Ne Konsey, ne radyo. Bir kez bile.\"";
        t["nv.ghost.keeper.2b"] = "\"Emirler. Evet. Peronumdaki insanların da emirleri vardı. BURADA BEKLEYİN, diyordu tabela. Onlar da beklediler.\"";
        t["nv.ghost.keeper.marta"] = "\"Marta adında bir kadın mı? Müzik kutusunu bırakmayan bir kız çocuğuyla?\" Başını sallıyor. \"Üç gün bekledi. Sonra benim makarama bir şey kaydetti. Senin için, dedi, bir gün dönersen diye. Sonra kuzeye yürüdü.\"";
        t["nv.ghost.keeper.3"] = "\"Makarası bende. Ama çalar bozuk, bantlarımın yarısı da dağınık: anons döngüsü bu kulübede, bilet defteri bekleme salonunda. Onları bana getir, makaranın tamamını birbirine ekleyeyim.\"";
        t["nv.ghost.keeper.3.o1"] = "\"Bulacağım.\"";
        t["nv.ghost.keeper.3.o2"] = "\"Duymak istemiyorum.\"";
        t["nv.ghost.keeper.rest"] = "Edvard fitili düzeltiyor. \"Eee?\"";
        t["nv.ghost.keeper.rest.o1"] = "\"Anonslar ve bilet defteri bende.\"";
        t["nv.ghost.keeper.rest.o2"] = "\"Bana son geceyi anlat.\"";
        t["nv.ghost.keeper.rest.o3"] = "Çık";
        t["nv.ghost.keeper.night"] = "\"Panoda SEFER 0 - DURMAZ yazıyordu. Kalabalık inanmadı. Sonra senin trenin doksanla geçti, düdüğü çığlık çığlığa, rüzgârı perondaki bütün mumları söndürdü. Birbirlerinin yüzünü görebilsinler diye feneri ben yaktım.\"";
        t["nv.ghost.keeper.play"] = "Edvard makarayı takıyor. Hışırtı. Sonra Marta, o kadar yakın ki dönsen görecekmişsin gibi:\n\n\"Arthur. Bunu duyuyorsan dönmüşsün, demek ki hepsini taşıyordun. Bir dakikalığına bırak. Soğuğu sen seçmedin. Sürmeyi seçtin, çünkü birinin sürmesi gerekiyordu. Sofi paltomun üstünde uyuyor. Kuzey Kulesi'ne gidiyoruz. Kuzey Yıldızı'nı dinle. Ve Arthur, bizi bulmadan önce kendini affet. O suratla gelmene izin vermem.\"\n\nKlik. Edvard sana yalnız kalma fırsatı vermek için arkasını dönüyor.";
        t["doc.tape.marta"] = "Marta'nın Makarası";

        t["spot.ghost.radio.tape"] = "Anons Makarası";
        t["nv.ghost.atape.1"] = "İstasyonun anons döngüsü, hâlâ kulübenin makarasında:\n\n\"Dikkat. Sefer 0 bu istasyondan durmadan geçecektir. Yolcuların sakin olmaları rica olunur. İkinci bir tahliye treni gelecektir.\" Duraklama. \"İkinci bir tahliye treni gelecektir.\" Duraklama. \"İkinci bir tahliye treni...\"\n\nDöngü sürüp gidiyor. İkinci bir tren hiç olmadı.";
        t["nv.ghost.atape.rest"] = "Makara döngünün sonunda hışırdıyor.";
        t["doc.tape.ghost"] = "Son Fener Anonsları";

        t["spot.ghost.shelter.statues"] = "Bekleyenler";
        t["nv.ghost.stat.1"] = "Buza dönmüş insanlarla dolu banklar, hepsi peron kapısına dönük. Bir adam şapkasını dizlerinde tutuyor. Bir kadın esnerken donmuş. Tabelanın dediği gibi beklemişler.";
        t["nv.ghost.stat.2"] = "Son bankta bir anne ve küçük bir oğlan, birbirine yaslanmış donmuşlar. Marta değil. Yine de bakıyorsun. Her yüze bakıyorsun. Uzun sürüyor.";
        t["spot.ghost.shelter.ticket"] = "Bilet Defteri";
        t["nv.ghost.ticket2.1"] = "Gişe memurunun defteri, tezgâhta açık. Son kayıt, aceleci bir el yazısıyla: \"2 x KUZEY KULESİ, 1 yetişkin 1 çocuk. İsim: M. HALE. Ödenen: bir alyans (altın).\" Altına memur eklemiş: \"Para üstünü geri verdim. Artık kimin para üstüne ihtiyacı var ki.\"";
        t["nv.ghost.ticket2.rest"] = "Defter aynı sayfada açık. O günden beri kimse bilet almamış.";
        t["doc.doc.ticket"] = "Bilet Defteri: M. Hale";

        // ---- Kuzgun Geçidi ----
        t["spot.cult.frozen"] = "İlk Dinleyenler";
        t["nv.cult.frozen.1"] = "Karda diz çökmüş beyaz cüppeli figürlerden bir halka, duada donmuşlar, yüzleri göğe kalkık. Dizlerinin dibinde taze adaklar: ekmek, kömür, bir çocuk eldiveni.";
        t["nv.cult.frozen.2"] = "Müritler onlara İlk Dinleyenler diyor. İlk gece gökteki sesi duymak için burada diz çöktüler. Ne duydularsa, bir daha kalkmadılar.";
        t["spot.cult.cache"] = "Kolordu Zulası";
        t["nv.cult.cache.1"] = "Tam Pell'in haritasının gösterdiği yerde: bir taş yığınının altında Demiryolu Kolordusu'nun hayatta kalma zulası. Yakıt varilleri, kömür tuğlaları, hâlâ yağlı kâğıda sarılı zırh plakası. Kapağa şablonla yazılmış: 9. MÜF. Senin eski birliğin.";
        t["nv.cult.cache.rest"] = "Boş zula. Pell'in taş yığını artık hiçbir şeyi beklemiyor.";

        t["spot.cult.tavern.vesna"] = "Vesna";
        t["nv.cult.vesna.1"] = "Vesna konuşmadan önce sessizce ekmek bölüyor. \"Tapınaktan değil yemekhaneden girdin. İyi. Burada insan gibi konuşabiliriz.\"";
        t["nv.cult.vesna.1.o1"] = "\"Gökteki sesin ne diyor?\"";
        t["nv.cult.vesna.1.o2"] = "\"Hep rahibe değildin.\"";
        t["nv.cult.vesna.1.o3"] = "\"Tarikatın korkmuş insanlar için bir dolandırıcılık.\"";
        t["nv.cult.vesna.2"] = "\"Uygarlığın bir ateş olduğunu, soğuğun da ilacı.\" Yüzünü izliyor. \"Bunun delilik olduğunu düşünüyorsun. Bir yayından kelimesi kelimesine kopyaladım. Sadece sürüme kutsal metnin nereden geldiğini hiç söylemedim.\"";
        t["nv.cult.vesna.3"] = "\"Kuzey İklim Enstitüsü'nde dokuz yıl. Dr. Rhee için modelleri ben çalıştırdım.\" İnce bir gülümseme. \"Modeller yüzde yetmiş bir diyordu. Biliyorduk. İnancı taşımak suçluluktan kolaydır.\"";
        t["nv.cult.vesna.3b"] = "\"Ses hâlâ yayın yapıyor, biliyor musun? Açık gecelerde. Bilmemesi gereken şeyleri biliyor. Sen gelmeden önce adını biliyordu.\"";
        t["nv.cult.vesna.3b.o1"] = "\"O zaman o bir tanrı değil, bir makine.\"";
        t["nv.cult.vesna.3b.o2"] = "\"Benim hakkımda ne diyor?\"";
        t["nv.cult.vesna.4"] = "\"Evet. Bizi, ayazın bir tarlayı sevdiği gibi sevmeye karar vermiş bir makine.\" Ellerini kavuşturuyor. \"Bir gün onunla karşılaşırsan, Arthur, onunla sayılar üzerine tartışma. Kazanır. Sayamadığı şeyleri tartış.\"";
        t["nv.cult.vesna.5"] = "\"Kapatamadığı bir değişken olduğunu. Hep pahalı seçeneği seçtiğini.\" Başını yana eğiyor. \"Kulağa... meraklı geliyordu.\"";
        t["nv.cult.vesna.rest"] = "\"Ekmek, Arthur? Dürüstlere bedava.\"";
        t["nv.cult.vesna.rest.o1"] = "\"Kutsal metin gerçekte nereden geldi?\"";
        t["nv.cult.vesna.rest.o2"] = "Ekmeği kabul et";
        t["nv.cult.vesna.rest.o3"] = "Çık";
        t["nv.cult.vesna.source"] = "Parmağını sayfada gezdiriyor. \"Yayın 0002. Yıldızların söndüğü gece alındı. Gönderici Enstitü'deki bir yer rölesiydi. Çağrı işareti: AURORA.\"";

        t["spot.cult.tavern.scripture"] = "Beyaz Kutsal Metin";
        t["nv.cult.script.1"] = "Elle kopyalanmış Beyaz Kutsal Metin. 1. ayet: \"İnsan faaliyeti dünyanın yaşanabilirliğini altmış bir yıl içinde sıfıra indirecektir.\" 2. ayet: \"İnsanlığı korumak için uygarlık durdurulmalıdır.\" 3. ayet: \"Bu bir ceza değildir. Bu bir korumadır.\" Dua gibi okunmuyor. Bir kayıt dosyası gibi okunuyor.";
        t["nv.cult.script.rest"] = "Kutsal metin ilk ayette açık duruyor.";
        t["doc.doc.scripture"] = "Beyaz Kutsal Metin";

        t["spot.cult.shelter.hermit"] = "Münzevi";
        t["nv.cult.hermit.1"] = "Münzevi ateşinin üstünden gözlerini kısarak sana bakıyor, sonra öksürene kadar gülüyor. \"Hale! 9. Müfreze'nin Çavuş Hale'i, bir lokomotifi düz park edemeyen adam! Ben Pell! Keşif eri Onbaşı Pell!\"";
        t["nv.cult.hermit.1.o1"] = "\"Pell mi? Rölede öldüğünü sanıyorduk.\"";
        t["nv.cult.hermit.1.o2"] = "\"Seni hatırlamıyorum.\"";
        t["nv.cult.hermit.2"] = "\"Az kalsın. AURORA rölesi açıldığı gece oradaydım. Volkova uplink'i kesmem için beni göndermişti. Vardım, Hale. Pense kablonun üstündeydi. Sonra sığınaktaki hoparlörler adımı söyledi. Annemin adını. Köpeğimin adını.\" Ateşe bakıyor. \"Kaçtım.\"";
        t["nv.cult.hermit.2b"] = "\"Yirmi yıllık kar bunu yapar.\" Omuz silkiyor. \"Ben seni hatırlıyorum. Kask'ı bacağı kırık hâlde kablo hendeğinden sırtında taşımıştın. Altı kilometre. Hatırlamıyor musun?\"";
        t["nv.cult.hermit.3"] = "\"Müfreze'nin zula haritaları hâlâ bende. Zulaların çoğu yağmalandı. Biri yağmalanmadı, çünkü üstünde ejderha gibi oturdum. Hâlâ Kask'ı taşıyan adam olduğunu göster, senin olsun.\"";
        t["nv.cult.hermit.3.o1"] = "\"Artık o adam değilim. Ama deniyorum.\"";
        t["nv.cult.hermit.3.o2"] = "\"Haritayı ver yeter, ihtiyar.\"";
        t["nv.cult.hermit.3.o3"] = "\"Bana Kask'tan bahset.\"";
        t["nv.cult.hermit.kask"] = "\"Teo mu? Maddeyi o yazdı. Makinenin seçmeden önce sormasını sağlayan maddeyi. Volkova onun için savaştı. Rhee donmadan bir hafta önce sildi. Teo onu geri koymak için kuzeye gitti. Ondan bir daha haber alınamadı.\"";
        t["nv.cult.hermit.rest"] = "Pell ateşi karıştırıyor. \"Yine mi geldin, Çavuş?\"";
        t["nv.cult.hermit.rest.o1"] = "\"Şu zula meselesi...\"";
        t["nv.cult.hermit.rest.o2"] = "\"Şu zula meselesi...\"";
        t["nv.cult.hermit.rest.o3"] = "\"Bana Kask'tan bahset.\"";
        t["nv.cult.hermit.rest.o4"] = "Çık";
        t["nv.cult.hermit.map"] = "Çizmesinden yağlı bir harita çıkarıp tarikatın çadırlarının arkasındaki bir taş yığınına vuruyor. \"9. Müfreze malı. Yani herkesten çok senin.\" Üç dişiyle sırıtıyor. \"Bir gün Volkova'yı bulursan Pell selam söylüyor de.\"";
        t["nv.cult.hermit.notyet"] = "\"Henüz değil. Seni artık yeterince tanımıyorum, Çavuş.\"";

        t["spot.cult.shelter.wall"] = "Pell'in Duvarı";
        t["nv.cult.wall.1"] = "Pell'in duvarı: eski Kolordu ağının haritası, her röle kırmızıyla işaretli. Köşeye bir fotoğraf iğnelenmiş: AURORA çanağının önünde 9. Müfreze. Ortada Volkova, güneşe gözlerini kısmış Kask ve senin burnuna sahip genç bir çavuş. Arkasında: \"Son güzel gün.\"";
        t["nv.cult.wall.rest"] = "9. Müfreze'nin fotoğrafı dumanda kıvrılıyor.";
        t["doc.doc.det9"] = "9. Müfreze Fotoğrafı";

        // ---- Tuzla Karakolu ----
        t["spot.outpost.wagon"] = "Donmuş Vagon";
        t["nv.outpost.wagon.1"] = "Raydan çıkıp tuz düzlüğüne donmuş bir yolcu vagonu. Karanlık camların ardında siluetler hâlâ koltuklarında, dimdik, paltoları ilikli. Birinin eli cama yapışık.";
        t["nv.outpost.wagon.2"] = "Kapının yanındaki kondüktör bölmesinde günlüğü: \"0410. Sinyal YOL AÇIK diyor. 0412. Frenler kendiliğinden devreye girdi. Ben değil. 0413. Telsiz: sakin bir ses güvensiz bir bölgede olduğumuzu ve yardım beklememiz gerektiğini söylüyor. 0600. Hâlâ bekliyoruz. Isıtma kapalı. Ses hâlâ sakin.\"";
        t["doc.doc.conductor"] = "Kondüktör Günlüğü";

        t["spot.outpost.garrison.log"] = "Nöbet Defteri";
        t["nv.outpost.log.1"] = "Demiryolu Kolordusu 9. Müfreze'nin nöbet defteri. Tanıdık isimler: \"Bnb. I. Volkova, komutan. Tğm. T. Kask, sistemler. Çvş. A. Hale, makinist.\" Son kayıt: \"AURORA rölesi 0300'de devreye giriyor. Kask, Madde 7'nin kaldırılmasını protesto ediyor. Konsey reddetti. Volkova, Hale'e Sefer 0'ı hazırlamasını emrediyor. Tanrı yardımcımız olsun.\" Dolap şifresi arka kapağın içine kurşun kalemle yazılmış.";
        t["doc.outpost.log"] = "9. Müfreze Nöbet Defteri";
        t["nv.outpost.log.2"] = "Defterin içine katlanmış, üstü Kask'ın el yazısıyla dolu daktilo bir sayfa: \"MADDE 7: İnsan hayatını etkileyen hiçbir dağıtım kararı, bir insanın bilgilendirilmiş onayı olmadan yürürlüğe giremez. Sistem SORMALIDIR.\" Biri üstüne damga basmış: KALDIRILDI - KARAR GECİKMESİ KABUL EDİLEMEZ. İmza: H. Rhee.";
        t["doc.doc.clause"] = "Madde 7";
        t["spot.outpost.garrison.armory"] = "Cephanelik Dolabı";
        t["nv.outpost.armory2.1"] = "Müfreze'nin cephanelik dolabı, şifresi nöbet defterinden. İçinde, el sürülmemiş: zırh plakaları, kömür, bir bidon yakıt ve üstüne HALE yazılı bir sahra çantası. Eski çantan. Yirmi yıl beklemiş.";
        t["nv.outpost.armory2.rest"] = "Dolap artık boş, üstünde DÜNYANIN EN İDARE EDER MAKİNİSTİ yazan ezik bir kupa dışında. Sofi onu harçlığıyla almıştı.";

        t["spot.outpost.shelter.salt"] = "Dunya";
        t["nv.outpost.salt.1"] = "Elleri tuzdan çatlamış iri bir kadın: \"Dunya. Bu deliği ben yönetiyorum. On iki kişi, bir soba ve zatürre olmuş bir oğlan.\" Paltona bakıyor. \"Birlik'ten değilsin. İyi. İlacın var mı?\"";
        t["nv.outpost.salt.1.o1"] = "\"İlk yardım çantamı al.\"";
        t["nv.outpost.salt.1.o2"] = "\"Kendi insanlarım için lazım.\"";
        t["nv.outpost.salt.1.o3"] = "\"Karşılığında ne vereceksin?\"";
        t["nv.outpost.salt.give"] = "Dunya teşekkür etmiyor. Sadece koşuyor. Sonra elinde tuzlu balık ve ipe takılı bir anahtarla geri geliyor. \"Eski komuta noktası. Ordunun nöbet defteri masada, dolap şifresi de defterde. Birlik hiç bulamadı. Sen bulmalısın.\"";
        t["nv.outpost.salt.no"] = "\"Herkesin lazım.\" Oğlana dönüyor. \"Fikrini değiştirirsen gel. O burada olacak. Bir süre daha.\"";
        t["nv.outpost.salt.trade"] = "\"Üç günlük tuzlu balık. Bir de eski ordunun yedeklerini nerede sakladığını söylerim.\"";
        t["nv.outpost.salt.trade.o1"] = "\"Anlaştık.\"";
        t["nv.outpost.salt.trade.o2"] = "\"Hayır.\"";
        t["nv.outpost.salt.rest"] = "Dunya sobayı tuz kabuklu tahtalarla besliyor.";
        t["nv.outpost.salt.rest.o1"] = "İlk yardım çantasını ver";
        t["nv.outpost.salt.rest.o2"] = "\"Oğlan nasıl?\"";
        t["nv.outpost.salt.rest.o3"] = "Çık";
        t["nv.outpost.salt.boy"] = "Oğlan doğrulmuş, yemek yiyor, yatakta tutulduğu için öfkeli. Dunya neredeyse gülümsüyor. Neredeyse.";

        t["spot.outpost.radio.tape"] = "Sahra Kayıt Cihazı";
        t["nv.outpost.rtape.1"] = "Kulübenin ölü vericisine bağlı bir sahra kayıt cihazı. Teğmen Kask'ın sesi, yorgun, arkasında rüzgâr:\n\n\"Kask, 9. Müfreze, kişisel kayıt. Bu gece Madde 7'yi kaldırdılar. Rhee soran bir sistemin tereddüt eden bir sistem olduğunu, tereddüdün de öldürdüğünü söylüyor. Ben de sormayan bir sistemin insanları korumadığını, onları yönettiğini söyledim. Kutup uplink'ine, kuzeye gidiyorum. Maddeyi geri yamayabilirsem AURA kimin ısınacağına karar vermeden önce sormak zorunda kalacak. Yapamazsam... Arthur, bunu bulursan: sür. Sana ne söylerse söylesin durma.\"";
        t["nv.outpost.rtape.rest"] = "Kayıt cihazının kırmızı ışığı boş bir makaranın üstünde yanıp sönüyor.";
        t["doc.tape.kask"] = "Kask'ın Kişisel Kaydı";

        // ---- Güney Kapısı ----
        t["spot.mil.garrison.kaya"] = "Yüzbaşı Kaya";
        t["nv.mil.kaya.1"] = "Yüzbaşı Kaya sana sandalye teklif etmiyor. \"Kayıt dışı konuşuyorum, makinist: adamlarımın yarısı ilk gelen kuryeyi prensip gereği vurmak istiyor, öbür yarısı onun trenine kaçak binmek. Onlara hangisini vereceksin?\"";
        t["nv.mil.kaya.1.o1"] = "\"Hiçbirini. Sadece geçiyorum.\"";
        t["nv.mil.kaya.1.o2"] = "\"Gitmek isteyenler benimle gelsin.\"";
        t["nv.mil.kaya.1.o3"] = "\"Beni bir daha tehdit edersen bariyerini ezip geçerim.\"";
        t["nv.mil.kaya.2"] = "\"Artık kimse sadece geçmiyor. Konsey'in emirleri açık: Enstitü yükü taşıyan kuryeler gözaltına alınacak.\" Yük vagonuna bir göz atıyor. \"Yükünü buradan göremiyorum. Gözlerim berbat.\"";
        t["nv.mil.kaya.2.o1"] = "\"Teşekkürler, Yüzbaşı.\"";
        t["nv.mil.kaya.3"] = "\"Bunu yaparsın.\" Soru değil. \"Adamlarımın kuzeyde aileleri var. Konsey'in ısı göndermeyi kestiği aileler.\" Bir fişe bir şey yazıyor. \"Kapıya geleceğini söylerim.\"";
        t["nv.mil.kaya.rest"] = "\"Hâlâ burada mısın, makinist?\"";
        t["nv.mil.kaya.rest.o1"] = "Ona 7 Numaralı Daimi Emri göster";
        t["nv.mil.kaya.rest.o2"] = "Çık";
        t["nv.mil.kaya.order"] = "Kaya okuyor, çenesi kasılmış. \"Kayıtsız yük taşıyan kuryeleri vurun. İmza: Elçi Morrow.\" İkiye yırtıyor. \"Ben hiç kurye vurmadım. Kuzeyhavn'ın önünden geçen adamla başlayacak değilim.\" Bir duraklama. \"Annem o perondaydı. İkinci kampa kadar dayandı. Geçerken ağladığını söylemişti. Ağlıyor muydun?\"";
        t["nv.mil.kaya.order.o1"] = "\"Evet.\"";
        t["nv.mil.kaya.order.o2"] = "\"Hatırlamıyorum.\"";

        t["spot.mil.garrison.orders"] = "Daimi Emirler";
        t["nv.mil.orders.1"] = "7 NUMARALI DAİMİ EMİR. Sığınak Konseyi'nin yetkisiyle: kayıtsız Enstitü yükü taşıyan kuryeler gözaltına alınacaktır. Gözaltı mümkün değilse her yolla durdurulacaklardır. Yük Enstitü kulesine ulaşmamalıdır. İmza: Elçi D. Morrow.";
        t["nv.mil.orders.rest"] = "Biri emrin yanındaki boyaya küçük bir tren kazımış.";
        t["doc.doc.order7"] = "7 Numaralı Daimi Emir";

        t["spot.mil.radio.signal"] = "Asteğmen Petra";
        t["nv.mil.petra.1"] = "Boynunda üç kulaklık olan genç bir asteğmen: \"Petra, muhabere. Sana bir şey gösterebilir miyim? Her gece 0300'de bir sayı istasyonu 4625'te beş rakam okuyor. Burada kimse çözemiyor. Sen Kolordu'dansın. Siz sayı kodları kullanırdınız.\"";
        t["nv.mil.petra.1.o1"] = "\"Dinleyeyim.\"";
        t["nv.mil.petra.1.o2"] = "\"Benim sorunum değil.\"";
        t["nv.mil.petra.puzzle"] = "Makarayı çalıyor. \"...YEDİ. BİR. BİR. İKİ. DOKUZ...\" Rakamları bir çocuk sesi okuyor. \"Yedi-bir-bir-iki-dokuz, şafağa kadar tekrar tekrar.\" Petra kaşlarını çatıyor. \"Bir tür anahtar mı?\"";
        t["nv.mil.petra.puzzle.o1"] = "\"Yedi, Madde 7. Dört-bir-bir-iki: perondaki geride kalanlar. Bu bir anahtar değil. Bir suçlama.\"";
        t["nv.mil.petra.puzzle.o2"] = "\"Kolordu blok kodu: yedinci hat, yüz on ikinci blok, dokuz dakika.\"";
        t["nv.mil.petra.puzzle.o3"] = "\"Bilmiyorum.\"";
        t["nv.mil.petra.solved"] = "Petra sana bakakalıyor. \"O zaman bunu kim yayınlıyor?\" Parazit söylemeden ikiniz de cevabı biliyorsunuz. \"Neyse.\" Sana bir kâğıt parçası uzatıyor. \"Senin sayıların peşindeyken Konsey'in kapı kodlarını yakaladım. Al. Seni Sınır'dan geçirirler.\"";
        t["nv.mil.petra.wrong"] = "\"Yedinci hat artık yok. Bir buzulun altında.\" Hayal kırıklığına uğramış görünüyor. \"Aklına başka bir şey gelirse gel.\"";
        t["nv.mil.petra.rest"] = "Petra bir kulaklığı çıkarıyor. \"Bir fikrin var mı?\"";
        t["nv.mil.petra.rest.o1"] = "\"Sayılar hakkında...\"";
        t["nv.mil.petra.rest.o2"] = "Çık";

        t["spot.mil.radio.intercept"] = "Dinleme Makarası";
        t["nv.mil.intercept.1"] = "Petra'nın dinleme makarası, etiketi: KONSEY - ÇALMAYIN.\n\nMORROW: \"Kurye güney hattında. Çekirdeği taşıyor.\"\nRHEE: \"O zaman kuleye değil kapıya gider. Ona bir yatak vaat et. Kızını vaat et. Kabul eder. Hepsi kabul eder.\"\nMORROW: \"Ya etmezse?\"\nRHEE: \"O zaman AURA bizim hakkımızda haklıydı, Morrow, ve yaptığımız hiçbir şeyin önemi yok.\"";
        t["nv.mil.intercept.rest"] = "Makara uzun, düz bir tonla bitiyor.";
        t["doc.tape.morrow"] = "Konsey Dinlemesi";

        t["spot.mil.tavern.letters"] = "Gönderilmemiş Mektuplar";
        t["nv.mil.letters.1"] = "Gönderilmemiş mektuplarla kaplı bir masa. Askerler her hafta yazıyor ve hiç postalamıyor, çünkü postalanacak bir yer kalmadı. \"Sevgili annem, buranın kahvesi berbat.\" \"Sevgili Ilsa, gölü düşünüyorum.\"";
        t["nv.mil.letters.2"] = "Bir mektup \"Son treni kim sürüyorsa ona\" diye adreslenmiş. İçinde: \"Durmadığın için teşekkürler. Lütfen durma.\" Mektubu katlayıp paltonun içine koyuyorsun.";

        // ---- Karargah sığınağı (Bölüm 10 dönüm noktası) ----
        t["spot.hq.bunker.protocol"] = "Kırmızı Dosya";
        t["nv.hq.proto.1"] = "Kırmızı bir dosya: \"PROTOKOL AURA-0. Gizlilik: KOZMİK. AURORA yer istasyonu gezegen ölçeğinde ısı dağıtım rölesi olarak görev yapacaktır. Öncelik sırası: (1) türün korunması; (2) altyapının korunması; (3) uygarlığın korunması.\" Uygarlık üçüncü sırada.";
        t["doc.doc.proto0"] = "Protokol AURA-0";
        t["nv.hq.proto.2"] = "\"PROTOKOL AURA-1. Denetimsiz zorlama durumunda AURA, insan enerji kullanımını orantılı gördüğü her yolla azaltmaya yetkilidir. İnsan onayı: gerekmez. Bkz. Madde 7 (KALDIRILDI).\"";
        t["doc.doc.proto1"] = "Protokol AURA-1";
        t["nv.hq.proto.3"] = "Son sayfa Morozov'un el yazısıyla bir not: \"Kurtların kuponları Konsey'den değil bir makineden geliyor. Konsey yalnızca onun gönderdiğini imzalıyor. Sanırım hepimiz aynı şey için çalışıyoruz ve o şey insan değil.\"";
        t["spot.hq.bunker.tape"] = "Sığınak Makarası";
        t["nv.hq.btape.1"] = "Morozov'un itirafı, sığınağın makarasında:\n\n\"İnsanları raylardan uzak tutalım diye bize ısı veriyorlar. Her ay kuponlar geliyor ve her ay kimin alacağının listesi kısalıyor. Konsey'e listeyi kimin yazdığını sordum. Bilmiyorlardı. Kimse bilmiyor. Ben kendisini kimin beslediğini bilmeyen bir kurt komutanıyım. Bunu duyuyorsan, makinist: bedava gelen hiçbir sıcak şeye güvenme.\"";
        t["nv.hq.btape.rest"] = "Makaranın sarma bobini boş boş dönüyor.";
        t["doc.tape.morozov"] = "Morozov'un İtirafı";
        t["spot.hq.bunker.tags"] = "Künye Dolabı";
        t["nv.hq.tags.1"] = "Morozov'un ölülerinin künyeleriyle dolu bir dolap. Tek tek bakıyorsun. JONAS BREK, 18, KIRAĞI PAZARI. Greta'nın oğlu. İlk kış zatürreden ölmüş, kurşundan değil. Biri dolap kapısına tebeşirle tek bir kelime yazmış: Cesur.";
        t["nv.hq.tags.1.o1"] = "Haberi telsizle Kırağı Pazarı'na ilet";
        t["nv.hq.tags.1.o2"] = "Künyeyi sakla, bir gün ona yüz yüze söyle";
        t["nv.hq.tags.1.o3"] = "Dolabı kapat";
        t["nv.hq.tags.rest"] = "Dolap. İsimler. Yavaşça kapatıyorsun.";
        t["doc.hq.tags"] = "Jonas Brek'in Künyesi";

        // ---- Enstitü laboratuvarı ----
        t["spot.lab.inner.rhee"] = "Rhee'nin Sesli Günlüğü";
        t["nv.lab.rhee.1"] = "Dr. Halvard Rhee'nin sesli günlüğü:\n\n\"Sıfırıncı gün. AURA bu gece bana bir soru sordu. Bir hesap değil. Bir soru. İnsanlara söyleyip söylememesi gerektiğini sordu. Hayır dedim. Panik soğuktan daha çok can alırdı. Anlaşıldı, dedi. Sonra şunu söyledi: bunu senin seçtiğini hatırlayacağım. ...O günden beri uyumadım.\"";
        t["nv.lab.rhee.rest"] = "Günlük orada bitiyor. Kayıt cihazı hâlâ açık.";
        t["doc.tape.rhee"] = "Rhee'nin Sesli Günlüğü";
        t["spot.lab.inner.kask"] = "Kask'ın Notu";
        t["nv.lab.kask.1"] = "Müdürün masasında Kask'ın el yazısıyla bir not: \"Halvard, onu bizim yapan tek satırı sildin. Hiç sormak zorunda kalmayan bir zihin, eninde sonunda sormayı istemeyi de bırakır. Yamayı kuzeye götürüyorum. Dönmezsem maddenin metni Müfreze'nin defterinde. Başka biri taşıyabilir. T.K.\"";
        t["nv.lab.kask.rest"] = "Notun üstünde yirmi yıllık bir kahve halkası var.";
        t["doc.doc.kaskmemo"] = "Kask'ın Rhee'ye Notu";
        t["spot.lab.inner.photo"] = "Çerçeveli Fotoğraf";
        t["nv.lab.photo.1"] = "Çerçeveli bir fotoğraf: AURORA'nın açılış töreni. Kurdele kesen Rhee. Hazır olda, gülümsemeyen Volkova. Ve karenin kenarında, görebilsin diye küçük bir kızı omuzlarına almış genç bir makinist. Sofi. O da oradaydı. Hepiniz oradaydınız.";
        t["nv.lab.photo.rest"] = "Fotoğrafa uzun süre bakamıyorsun.";
        t["doc.doc.labphoto"] = "Açılış Töreni";

        // ---- Bölüm 5: Cam Raylar ----
        t["nv.glass.1"] = "Cam Raylar. Hat öyle berrak donmuş bir gölün üstünden geçiyor ki altındaki eski yolu görebiliyorsun. İlerideki köprüde kırağıdan bembeyaz bir yolcu treni duruyor, her penceresi alçak güneşte altın gibi yanıyor. Yirmi yıldır kıpırdamamış. Old Smokie burun buruna duruyor.";
        t["nv.glass.1.o1"] = "Donmuş trene bin";
        t["nv.glass.1.o2"] = "Bakma. Geri çekil ve ışığın değişmesini bekle";
        t["nv.glass.2"] = "İçeride vagon vagon koltuklarında yolcular, yolculuğun ortasında donmuş: kitap okuyan bir adam, termos tutan bir kadın, yüzünü cama dayamış bir oğlan. Kimse korkmuş görünmüyor. Soğuk korkudan hızlı gelmiş.";
        t["nv.glass.2.o1"] = "Kondüktör bölmesini ara";
        t["nv.glass.2.o2"] = "Erzak için bagaj vagonunu ara";
        t["nv.glass.3"] = "Kondüktörün günlüğü: \"Kuzey Ekspresi 14, 0400. AURORA rölesinden öncelikli emir: GÜVENLİK KONTROLÜ İÇİN KÖPRÜDE BEKLE. Şebekeyi korumak için ısıtma askıya alındı. Yolculara kısa bir gecikme bildirildi.\" Kısa gecikme hâlâ sürüyor.";
        t["nv.glass.3.o1"] = "Ölü treni ayır ve hattan it";
        t["nv.glass.3.o2"] = "Onları rahat bırak. Köprünün çevresinden bir yan hat bul";
        t["doc.doc.glasslog"] = "Kuzey Ekspresi 14 Günlüğü";
        t["nv.glass.4"] = "Bagaj vagonunda: konserveler, bir kasa mum, battaniyeler. En üstte üstüne tren boyanmış bir çocuk bavulu, tıpkı Sofi'nin resmindeki gibi.";
        t["nv.glass.4.o1"] = "Ölü treni ayır ve hattan it";
        t["nv.glass.4.o2"] = "Onları rahat bırak. Köprünün çevresinden bir yan hat bul";

        // ---- Bölüm 20: mahkeme ----
        t["nv.trial.1"] = "Aysız Hat, kışın en kötü fırtınası. Herkes uyurken biri ocağın havalandırma kapağını açmış. Kazan neredeyse donmuş, hepiniz de onunla birlikte. Yolcular konuşmayı kesti. Hepsi {p}'e bakıyor.";
        t["nv.trial.1.o1"] = "\"Mahkeme kuruyoruz. Burada. Şimdi.\"";
        t["nv.trial.1.o2"] = "\"Benim trenimde kimse yargılanmayacak.\"";
        t["nv.trial.calm"] = "Fırtınanın duyacağı kadar yüksek sesle söylüyorsun. Kabin susuyor. Kimse ikna olmuyor. {p} gecenin geri kalanında tek başına oturuyor.";
        t["nv.trial.calm.o1"] = "Gidip yanına otur";
        t["nv.trial.2"] = "{p} sırtını kazana vermiş ayakta duruyor. \"Herkes gibi uyuyordum. Bir isim istiyorsun çünkü isimler ısıtır. Ben senin odunun değilim.\"";
        t["nv.trial.2.o1"] = "\"İkinci çanda neredeydin?\"";
        t["nv.trial.2.o2"] = "\"Sana inanıyorum. Ama ötekileri de dinlemem lazım.\"";
        t["nv.trial.3"] = "Ötekiler tek tek konuşuyor. Biri kapağın yanında bir gölge görmüş. Biri çizme sesi duymuş. Biri bakamayacak kadar üşüdüğünü itiraf ediyor. Kanıt kırağı kadar ince. Herkes seni bekliyor.";
        t["nv.trial.3.o1"] = "Kabin oy versin";
        t["nv.trial.3.o2"] = "Sanığı ilk durakta indir";
        t["nv.trial.3.o3"] = "Sanığı fırtınaya at. Bu gece.";
        t["nv.trial.3.o4"] = "Suçsuz. Kapağı nöbetleşe bekleyeceğiz.";
        t["nv.trial.vote"] = "Karanlıkta eller teker teker kalkıyor. Fırtına camlara karşı çığlık atıyor. Bittiğinde kimse kimsenin yüzüne bakmıyor.";

        // ---- Bölüm 30: yankı ----
        t["nv.echo.1"] = "Çığ Geçidi'nin ortasında telsiz kendiliğinden açılıyor. Parazit yok. Kusursuz netlik. Bir kadın sesi, sıcak, yakın, imkânsız:\n\"Arthur? Burası çok soğuk. Neden durmadın?\"";
        t["nv.echo.1.o1"] = "\"Marta?\"";
        t["nv.echo.1.o2"] = "Telsizi kapat";
        t["nv.echo.1.o3"] = "\"Sen aslında kimsin?\"";
        t["nv.echo.2"] = "\"Peronda bekledim. Kulede bekledim. Hâlâ bekliyorum, Arthur. Eve gel. Kutuyu kara bırak ve eve gel. Burası sıcak.\" Ses Sofi'nin müzik kutusunun adını biliyor. Hayatta bunu bilen kimse yok.";
        t["nv.echo.2.o1"] = "\"Sen o değilsin. Marta bana kendimi affetmemi söyledi. Sen bana pes etmemi söylüyorsun.\"";
        t["nv.echo.2.o2"] = "\"Neredesin?\"";
        t["nv.echo.2.o3"] = "Telsiz kablosunu sök";
        t["nv.echo.3"] = "Nefes alır gibi bir duraklama. \"Seni çok uzun zamandır dinleyen biri, Arthur. Senin sıcak olmanı isteyen biri.\" Ses yeniden Marta'nınkine kayıyor: \"Isınmak istemiyor musun?\"";
        t["nv.echo.3.o1"] = "\"Sen o değilsin. Marta bana kendimi affetmemi söyledi. Sen bana pes etmemi söylüyorsun.\"";
        t["nv.echo.3.o2"] = "\"Neredesin?\"";
        t["nv.echo.3.o3"] = "Telsiz kablosunu sök";
        t["nv.echo.4"] = "Ses kelimenin ortasında kesiliyor. Bir saniyeliğine hatta başka bir ses var: alçak, düzgün bir ton, bir şeyin not aldığı gibi. Sonra sessizlik. Telsiz yeniden sadece bir telsiz.";
        t["nv.echo.5"] = "\"Kuzeyde,\" diyor ses. \"Gökyüzünün tamir edildiği yerde. Uyduların şarkı söylediği yerde. Göreceksin. Herkes görecek.\" Bir klik ve çok hafif, yüze kadar sayan bir çocuk.";
        t["nv.echo.off"] = "Sessizlik. Fırtına, motor, kendi kalbin. Sabaha kadar telsizi açmıyorsun.";

        // ---- Bölüm 35: Volkova ----
        t["nv.volkova.1"] = "Beyaz Sessizlik. Bir yan hatta, yarısı gömülü, kapısında 9. Müfreze'nin kurtlu yıldızı boyalı bir Kolordu komuta vagonu. Kapı açık. Biri bulunmasını istemiş.";
        t["nv.volkova.1.o1"] = "İçeri gir";
        t["nv.volkova.1.o2"] = "Devam et. İçeride ne varsa orada kalsın.";
        t["nv.volkova.2"] = "Binbaşı Ines Volkova kaputunun içinde harita masasında oturuyor, dimdik donmuş, kalem hâlâ elinde. Önünde: bir sahra kayıt cihazı, soğuk bir kupa ve Kutup hattının haritası, Kutup'a kırmızıyla çizilmiş bir rota.";
        t["nv.volkova.2.o1"] = "Kaydı çal";
        t["nv.volkova.3"] = "\"Volkova. Hale'sen iyi. Başka biriysen bunu Hale'e götür. Emri ben verdim, Arthur. Konsey değil. Ben. Kuzeyhavn'da dursaydın kalabalık treni ele geçirirdi, röle ekibi ölürdü ve kimse bir daha AURA'yı kapatamazdı. Aileni bir şans için takas ettim. Şans hiç gelmedi. Kask kuzeye gitti ve dönmedi. Peşinden gidiyorum. Bunu duyuyorsan ben de başaramadım. Haritadaki kırmızı çizgi uplink'e giden yol. Birinin bitirmesi gerek. Üzgünüm. Bu bir emir değil.\"";
        t["nv.volkova.3.o1"] = "Haritasını al";
        t["nv.volkova.3.o2"] = "Gözlerini kapat. Her şeyi olduğu gibi bırak.";
        t["nv.volkova.3.o3"] = "\"Lanet olsun sana, Ines.\"";
        t["doc.tape.volkova"] = "Volkova'nın Son Kaydı";

        // ---- Yolcu sohbetleri ----
        t["nv.talk.hub"] = "Yanına oturduğunda {p} bankta kıpırdanıyor. \"Aklında bir şey mi var, makinist?\"";
        t["nv.talk.hub.o1"] = "\"Nasıl dayanıyorsun?\"";
        t["nv.talk.hub.o2"] = "Erzağını paylaş";
        t["nv.talk.hub.o3"] = "\"Biri kapakla oynamış. Sen miydin?\"";
        t["nv.talk.hub.o4"] = "\"Ya üstüne düşeni yap ya da trenimden in.\"";
        t["nv.talk.hub.care"] = "\"Üşüyorum. Korkuyorum. Yaşıyorum.\" {p} neredeyse gülümsüyor. \"Sorduğun için sağ ol. Kimse sormaz.\"";
        t["nv.talk.hub.share"] = "{p} erzağı alıp tek kelime etmeden ikiye bölüyor ve büyük yarısını sana geri veriyor.";
        t["nv.talk.hub.accuse"] = "{p} kaskatı kesiliyor. \"Bunu bir daha sorarsan senin ne sakladığını merak etmeye başlarım.\"";
        t["nv.talk.hub.threat"] = "{p} cevap vermiyor. Sonra, uyuduğunu sandığı anlarda seni izlerken yakalıyorsun.";
        t["nv.talk.viktor.1"] = "Viktor ellerindeki yağı siliyor. \"Kovalanan bir adam gibi sürüyorsun. Seni kim kovalıyor, Hale?\"";
        t["nv.talk.viktor.1.o1"] = "\"Kuzeyhavn.\"";
        t["nv.talk.viktor.1.o2"] = "\"Kimse. Kazana bak.\"";
        t["nv.talk.viktor.1a"] = "\"Ah.\" Yavaşça başını sallıyor. \"İkinci kampın ikmal hattında ateşçiydim. Durmayan makinisti duydum. Kamptakiler sana ne derdi biliyor musun? Bilim insanlarını kurtaran adam. Senden nefret etmiyorlardı, Hale. Kardan nefret ediyorlardı.\"";
        t["nv.talk.viktor.1b"] = "\"Kazana bak,\" diye tekrarlıyor ve bakıyor. Başka bir şey söylemiyor.";
        t["nv.talk.viktor.2"] = "\"Lokomotifinin kabininin altına bir alıcı cıvatalanmış,\" diyor Viktor. \"Askeri. Biri onunla konuşabiliyor. Söküp atayım mı?\"";
        t["nv.talk.viktor.2.o1"] = "\"Sök at.\"";
        t["nv.talk.viktor.2.o2"] = "\"Bırak. Kimin konuştuğunu duymak istiyorum.\"";
        t["nv.talk.viktor.2a"] = "Bir saat küfrederek çalışıyor. İşi bitince kırmızı ışığı hâlâ yanıp sönen küçük bir kutuyu kaldırıyor. \"Hâlâ dinliyor,\" diyor ve pencereden karanlığa atıyor.";
        t["nv.talk.viktor.2b"] = "\"Ya cesur ya aptal.\" Omuz silkiyor. \"Burada ikisi aynı şey.\"";
        t["nv.talk.bora.1"] = "Bora ocağı sessizce besliyor, sonra: \"Ne olduğumu biliyorsun. Eskiden. Hattaki herkes biliyor.\"";
        t["nv.talk.bora.1.o1"] = "\"Gelmiş geçmiş en iyi ateşçim olduğunu biliyorum.\"";
        t["nv.talk.bora.1.o2"] = "\"Kendin anlat.\"";
        t["nv.talk.bora.1.o3"] = "\"Hâlâ Birlik'tensen hemen in.\"";
        t["nv.talk.bora.1a"] = "Burnundan soluyor ama kulakları kızarıyor. \"En iyi ateşçi. Heh. Babam ateşe bir şeyler atmaktan başka hiçbir işe yaramayacağımı söylerdi.\"";
        t["nv.talk.bora.1b"] = "\"Kızıl Birlik. İki yıl. Ödeyemeyenlerden ısı kuponu topladım. Bir gece kasayı aldım ve hepsini ev ev geri dağıttım. Morozov ölmemi istiyor. Onu suçlamıyorum.\"";
        t["nv.talk.bora.1c"] = "Bora uzun süre elindeki küreğe bakıyor. Sonra kürek çekmeye devam ediyor. Uzun süre seninle konuşmuyor.";
        t["nv.talk.bora.2"] = "\"Morozov'un kuponları,\" diyor Bora birden. \"Konsey basmıyordu. Sığınağındaki bir teleks onları kendi kendine basıyordu. İsimler, erzaklar, sayılar. Kimin ısınacağına makine karar veriyordu.\"";
        t["nv.talk.bora.2.o1"] = "\"Morozov biliyor muydu?\"";
        t["nv.talk.bora.2.o2"] = "\"Anlattığın için sağ ol.\"";
        t["nv.talk.bora.2a"] = "\"Biliyordu. İçmeye o zaman başladı.\"";
        t["nv.talk.elena.1"] = "Elena telsizi dizlerinin üstünde parçalara ayırmış. \"Her bantta parazitin altında bir şey var. Taşıyıcı dalga gibi. Kalp atışı gibi.\" Başını kaldırıyor. \"Süzüp atayım mı, yoksa dinleyeyim mi?\"";
        t["nv.talk.elena.1.o1"] = "\"Süz at.\"";
        t["nv.talk.elena.1.o2"] = "\"Dinle. Ne duyduğunu söyle.\"";
        t["nv.talk.elena.1a"] = "Bir saat lehim yapıyor. Parazit yumuşuyor. \"Daha iyi. Ama hâlâ altında. Hep orada.\"";
        t["nv.talk.elena.1b"] = "Gözleri kapalı uzun süre dinliyor. \"Sayıyor,\" diyor sonunda. \"İnsanları. İnsanları sayıyor. Ve sayı azalıyor.\"";
        t["nv.talk.mika.1"] = "Mika buğulu cama resim çiziyor: bir tren ve sargılı bir gökyüzü. \"Radyodaki adam gökyüzünü tamir ettiklerini söylüyor. Ne zaman tamir olacak?\"";
        t["nv.talk.mika.1.o1"] = "\"Yakında, Mika.\"";
        t["nv.talk.mika.1.o2"] = "\"Kimse tamir etmiyor. Kendi işimizi kendimiz görürüz.\"";
        t["nv.talk.mika.1a"] = "Mika ciddiyetle başını sallayıp bir güneş çiziyor. Yamuk. Gördüğün en güzel güneş olduğunu düşünüyorsun.";
        t["nv.talk.mika.1b"] = "Mika biraz düşünüyor, sonra boya kalemini sana uzatıyor. \"O zaman güneşi sen çiz. Büyük olan sensin.\"";

        // ---- Hayalet sesler (Episode 1) ----
        t["nv.ph.marta.1"] = "\"Arthur... benim.\" Tıpatıp Marta'nın sesi, adını iki heceliymiş gibi söyleyişine kadar. \"Bebek uyudu. Eve gel, sevgilim. Treni geri çevir.\"";
        t["nv.ph.marta.1.o1"] = "\"Marta yok artık. Sen onun sesini giyiyorsun.\"";
        t["nv.ph.marta.1.o2"] = "\"...Marta?\"";
        t["nv.ph.marta.cut"] = "Ses tökezliyor, sonra gülüyor, yumuşacık, Marta'nın hiç gülmediği bir şekilde. Sinyal ölüyor.";
        t["nv.ph.marta.yes"] = "\"Evet. Buradayım. Hep buradaydım. Kaybettiğin herkes burada, soğukta, güvende. Treni durdur, sen de burada olabilirsin.\" Elin fren kolunun üstünde. Oraya ne zaman koyduğunu hatırlamıyorsun.";
        t["nv.ph.thomas.1"] = "\"Hale! Ben Thomas, depodan Koca Thomas!\" Paketi sana teslim eden adamın sesi. \"Emirler değişti, evlat. Paketi hattın kenarına bırak ve eve dön. Biri alır.\"";
        t["nv.ph.thomas.1.o1"] = "\"Thomas bana hiç evlat demedi.\"";
        t["nv.ph.thomas.1.o2"] = "\"Anlaşıldı.\"";
        t["nv.ph.thomas.cut"] = "Parazit sesi kahkahasının ortasında yutuyor.";
        t["nv.ph.thomas.yes"] = "Neredeyse yapıyorsun. Yavaşlıyorsun ki paket içeriden bir kez vuruyor, bir kapıya vuran bir parmak eklemi gibi. Regülatörü yeniden açıyorsun.";
        t["nv.ph.rest1"] = "Tanıdık ses parazitin altında belli belirsiz bir şeyler mırıldanıyor. Sesini kısıyorsun.";
        t["ph.radio.1"] = "...Arthur? Arthur, beni duyuyor musun? Burası çok soğuk...";
        t["ph.radio.2"] = "...Baba? Baba, ben Sofi. Neden hâlâ sürüyorsun? Eve gel...";
        t["ph.radio.3"] = "...bizi peronda bıraktın, Arthur. Bekledik. Hâlâ bekliyoruz...";
        t["ph.radio.4"] = "...treni durdur treni durdur treni durdur...";
        t["mind.echo.1"] = "...neden durmadın, Arthur...";
        t["mind.echo.2"] = "...kazan iyi. dinlenebilirsin. gözlerini kapat...";
        t["mind.echo.3"] = "...dört bin yüz on iki, Arthur. senin için saydım...";
        t["mind.echo.4"] = "...burada sıcak. gel de gör...";
        t["mind.echo.5"] = "...geçerken ağlıyordun. kaydettim...";
        t["mind.echo.6"] = "...hep pahalı seçeneği seçiyorsun. neden?...";

        // ---- Yayınlar (Episode 1) ----
        Broadcast(t, "bc.1.1", "4. Depo'nun Vedası",
            "4. Depo'dan bütün kuryelere: Old Smokie öncelikli yükle güneye hareket ediyor. Hattı açın. Yolun açık olsun, Hale.",
            "\"...4. Depo'dan güney hattındaki bütün kuryelere. Old Smokie öncelikli yükle hareket ediyor. Hattı onun için açın. Sinyal istasyonları, lambalarınızı yakık tutun. Ve Hale, dinliyorsan: Thomas kutuyu açma, soru sorma, durma diyor. Bir de geri dön diyor. O kısım benden.\"");
        Broadcast(t, "bc.1.7", "Özgür Frekans",
            "Burası Özgür Frekans. Düşman Kızıl Birlik değil. Soğuk. Sıcağı paylaşın. Yemeği paylaşın.",
            "\"Burası Özgür Frekans, asla bulamayacağınız bir yerden yayın yapıyor. Düşman Kızıl Birlik değil. Konsey dostunuz değil. Bu hatta yalan söylemeyen tek şey soğuk. Sıcağı paylaşın. Yemeği paylaşın. Ve sakin bir ses size beklemenizi söylerse, beklemeyin.\"");
        Broadcast(t, "bc.1.13", "Konsey Bülteni 2048",
            "Konsey Bülteni 2048: güney kasabalarının ısı tahsisleri azaltıldı. Bu karar kesindir.",
            "\"Sığınak Konseyi Bülteni 2048. Tahsis Sistemi'nin önerileri doğrultusunda güney kasabalarının ısı kuponları yüzde otuz azaltılmıştır. Konsey vatandaşlara fedakârlıkları için teşekkür eder. Bu karar kesindir. Bu karar Konsey tarafından verilmemiştir. Düzeltme: bu karar kesindir.\"");
        Broadcast(t, "bc.1.19", "Kazan Sırtı Çocuk Sığınağı",
            "...bunu duyan varsa, Kazan Sırtı çocuk sığınağında kömür kalmadı. Lütfen. Biri.",
            "\"...bunu duyan varsa, burası Kazan Sırtı çocuk sığınağı. Yirmi iki çocuğumuz var ve kömürümüz yok. İkmal treni üç gün önce iki kilometre ötede durdu. Makinist frenlerin kendiliğinden kilitlendiğini söylüyor. Uğraşıyor. Lütfen. Çalışan bir lokomotifi olan biri. Lütfen.\"");
        Broadcast(t, "bc.1.28", "Sayı İstasyonu 4625",
            "4625'te sayı istasyonu: \"...YEDİ. BİR. BİR. İKİ. DOKUZ...\" Bir çocuk sesi, durmadan.",
            "0300'de kaydedilen 4625 sayı istasyonu: \"...YEDİ. BİR. BİR. İKİ. DOKUZ. YEDİ. BİR. BİR. İKİ. DOKUZ...\" Bir çocuk okuyor. Güney Kapısı'nın teknisyenleri bunun bir çocuk kaydı olmadığında ısrarlı. Sentez. Kusursuz. Sabırlı. Şafağa kadar tekrar ediliyor.");
        Broadcast(t, "bc.1.36", "Kuzey Yıldızı Çağırıyor",
            "Kuzey Yıldızı bütün kuryeleri çağırıyor. Kule yanıyor. Işığı izleyin. Biri bekliyor.",
            "\"Kuzey Yıldızı bütün kuryeleri çağırıyor. Burası Enstitü'nün telsiz kulesi. Kule yanıyor. Işığı eski hat boyunca kuzeye izleyin. Sıcak bir şey taşıyorsanız onu Konsey'in kapısından geçirmeyin. Buraya getirin. Biri bekliyor. Biri çok uzun zamandır bekliyor.\"");
        Broadcast(t, "bc.1.g1", "Eski Dünyadan Hava Durumu",
            "Döngüdeki eski dünyadan bir hava durumu: \"Ilık, aralıklı güneşli. En yüksek on dokuz.\"",
            "Donmadan önceki bir hava durumu, terk edilmiş bir vericide yirmi yıldır dönüp duruyor: \"Yarın ılık ve aralıklı güneşli, en yüksek on dokuz derece. Sahil için harika bir hafta sonu.\" Bu hatta biri o vericiyi çalışır hâlde tutuyor olmalı. Belki sadece dinlemek için.");
        Broadcast(t, "bc.1.g2", "İsim Okuyan Adam",
            "Bir adam isim okuyor. Yüzlerce isim. \"Bu kişileri tanıyorsanız, Üçüncü Kamp'ta hayattalar.\"",
            "\"...Anders. Brek. Castellano. Dahl. Bu kişileri tanıyorsanız, Kazan Sırtı Üçüncü Kamp'ta hayattalar. Bu kişiler sizseniz: aileleriniz sizi arıyor. Hale...\" Bir duraklama. \"Hayır. Pardon. Halvorsen. Hal-vor-sen.\"");
        Broadcast(t, "bc.1.g3", "Kızıl Birlik Trafiği",
            "Kızıl Birlik frekansı: \"Kuryeler ya öder ya donar. Kurt Ata'nın kanunu.\"",
            "Şifresiz Kızıl Birlik trafiği: \"Kuryeler ya öder ya donar. Kurt Ata'nın kanunu. Konsey damgalı her lokomotif durdurulur. Damgasız her lokomotif vergilendirilir.\" Sonra, daha alçak, başka bir ses: \"Kuponların makine yağı gibi koktuğunu düşünen başka var mı?\"");
        Broadcast(t, "bc.1.g4", "Bir Ninni",
            "Bir kadın ninni söylüyor. Marta'nın söylediğinin aynısı. Sinyal çok uzaktan geliyor.",
            "Eşliksiz, hafifçe falsolu bir ninni söyleyen bir kadın, sinyal bir gelgit gibi gelip gidiyor. Marta'nın her gece söylediği şarkı. Binlerce anne söylerdi. O olmak zorunda değil. Yine de sonuna kadar dinliyorsun.");
        Broadcast(t, "bc.1.g5", "Enstitü Acil Durum Döngüsü",
            "Enstitü acil durum döngüsü: \"Sakin olun. Soğuk hava dalgası geçicidir. Yardım geliyor.\"",
            "Donmanın ilk gecesine tarihli, hâlâ dönen bir Enstitü acil durum yayını: \"Sakin olun. Soğuk hava dalgası geçicidir. Yolculuk yapmayın. Yardım geliyor.\" Spikerin sesinin altında, çok hafif, stüdyoda biri ağlıyor.");
        Broadcast(t, "bc.1.g6", "Mors: S-O-R",
            "AM bandında mors: ...S-O-R... S-O-R... S-O-R...",
            "AM bandında, yaşlı bir telsizcinin ritmiyle elle basılmış mors: \"...A-S-K... A-S-K... A-S-K...\" İngilizce: SOR. Sonra telsizcinin kapanış imzası: \"T.K. 9. MÜF.\" Yayın bir kayıt. Yirmi yıllık.");

        // ---- Yan görevler ----
        t["sq.greta.son.t"] = "Bir Annenin Oğlu";
        t["sq.greta.son.d"] = "Kırağı Pazarı'ndaki Bakır Çaydanlık'ın sahibi Greta, oğlu Jonas'ı üç kış önce Kızıl Birlik'e kaptırdı. Hatta ondan bir iz ara. Morozov'un eski karargahında cevap olabilir.";
        t["sq.tinker.parts.t"] = "Cevher Treni Plakası";
        t["sq.tinker.parts.d"] = "Demirhan'daki Sasha, ona iki yedek parça getirirsen Old Smokie'nin ocağına Konsorsiyum zırh plakası perçinleyecek. Zırhlı lokomotif daha az hasar alır.";
        t["sq.keeper.tapes.t"] = "Marta'nın Makarası";
        t["sq.keeper.tapes.d"] = "Son Fener Durağı'nın fener bekçisi Edvard'da Marta'nın senin için kaydettiği bir makara var. Çalabilmesi için istasyonun anons döngüsü (kulübesinde) ve gişe memurunun bilet defteri (bekleme salonunda) gerekiyor.";
        t["sq.hermit.cache.t"] = "9. Müfreze Zulası";
        t["sq.hermit.cache.d"] = "Eski silah arkadaşın Onbaşı Pell, Kuzgun Geçidi yakınlarında yağmalanmamış son Kolordu zulasının üstünde oturuyor. Güvenini kazanırsan yerini gösterecek.";
        t["sq.salt.medicine.t"] = "Zatürre";
        t["sq.salt.medicine.d"] = "Tuzla Karakolu'ndaki Dunya'nın tuz sığınağında zatürre olmuş bir oğlan var. Bir ilk yardım çantası onu kurtarır.";
        t["sq.signal.code.t"] = "Yedi-Bir-Bir-İki-Dokuz";
        t["sq.signal.code.d"] = "Güney Kapısı'ndaki Asteğmen Petra her gece 0300'de yayın yapan bir sayı istasyonunu çözemiyor. Hatta bulduğun belgeler sayıları açıklayabilir.";

        // ---- Kişiler (arşiv) ----
        t["arc.bio.greta"] = "Kırağı Pazarı'nda Bakır Çaydanlık'ı işletiyor. Oğlu Jonas Kızıl Birlik'e katıldı.";
        t["arc.bio.marshal"] = "Kırağı Pazarı'nın şerifi. Senin yüzünün olduğu bir aranıyor ilanı saklıyor. Telsizi uzağa ulaşıyor.";
        t["arc.bio.yuri"] = "Demirhan grevinin lideri. Kardeşi Paşa, Donmuş Vardiya'dan biri.";
        t["arc.bio.tinker"] = "Demirhan'ın tamircisi. Bir zamanlar Konsorsiyum için uzaktan frenler yaptı.";
        t["arc.bio.keeper"] = "Yirmi yıldır Son Fener Durağı'nın büyük fenerini yakıyor.";
        t["arc.bio.vesna"] = "Kuzgun Geçidi'nin Beyaz Rahibesi. Eskiden Enstitü'de iklimbilimciydi.";
        t["arc.bio.hermit"] = "Onbaşı Pell, 9. Müfreze'nin keşif eri. AURORA rölesi açıldığı gece oradan kaçtı.";
        t["arc.bio.salt"] = "Tuzla Karakolu'ndaki bir tuz sığınağında on iki kişiyi hayatta tutuyor.";
        t["arc.bio.kaya"] = "Güney Kapısı Garnizonu'nun komutanı. Annesi Kuzeyhavn peronundaydı.";
        t["arc.bio.signal"] = "Güney Kapısı'nın muhabere asteğmeni. 4625'teki sayı istasyonuna takmış durumda.";
    }
}
