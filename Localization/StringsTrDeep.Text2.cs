using System.Collections.Generic;

namespace TheLastDelivery.Localization;

public static partial class StringsTrDeep
{
    private static void Episode2(Dictionary<string, string> t)
    {
        // ---- Konuşanlar, mekânlar ----
        t["spk.b17"] = "Servis Birimi B-17";
        t["spk.kask"] = "Teo Kask (Yeniden İnşa)";
        t["spot.ph.voices1"] = "Tanıdık Bir Ses";
        t["spot.ph.voices2"] = "Tanıdık Bir Ses";
        t["int.name.drone.RadioRoom"] = "Röle Kulübesi";
        t["int.name.drone.Garrison"] = "Makine Koğuşu";
        t["int.name.factory.Tavern"] = "Gece Vardiyası Kantini";
        t["int.name.factory.Workshop"] = "Tüp Atölyesi";
        t["int.name.factory.Shelter"] = "Çocuk Koğuşu";
        t["int.name.chapel.RadioRoom"] = "İletim Odası";
        t["int.name.chapel.Shelter"] = "Hacılar Salonu";
        t["int.name.tower.RadioRoom"] = "Kule Kontrol Odası";
        t["story.b17"] = "B-17 devriye çizelgesini hatırladı: Sentinel'lerin nereden vuracağını biliyorsun.";
        t["story.falseRoute"] = "Kiril'in sahte rotası işe yaradı: Sentinel'ler yanlış hatta bekliyor.";

        // ---- Episode 2 genel iç mekânlar ----
        t["spot.int.garrison.board2"] = "Direktif Ekranı";
        t["nv.int.board2.1"] = "Eskiden ilan panosunun olduğu yere cıvatalanmış bir ekran. Yavaşça dönüyor: SICAKLIK UYUMDUR. UYUM SICAKLIKTIR. BUGÜNKÜ ISI PAYINIZ: %18. BEKLEDİĞİNİZ İÇİN TEŞEKKÜRLER.";
        t["nv.int.board2.2"] = "Ekranın altına çelik üstüne çiviyle kazınmış: \"Teşekkür etti. Hep teşekkür ediyor. Teşekkür etmesinden nefret ediyorum.\"";
        t["nv.int.board2.rest"] = "Ekran beklediğin için teşekkür ediyor.";
        t["spot.int.shelter.cot2"] = "Ranza";
        t["nv.int.cot2.rest"] = "Uyuyorsun. AURA'nın taşıyıcı dalgası duvarlarda, senin olmayan bir kalp gibi uğulduyor.";
        t["doc.mem2.rest.1"] = "Paket";
        t["nv.mem2.rest.1"] = "Ranzada eski depoyu görüyorsun: Koca Thomas sana muşambaya sarılı bir paket uzatıyor. \"Açma, sorma, durma.\" \"Ya zorunda kalırsam?\" \"O zaman içinde ne olduğunu hepimizden iyi bilirsin.\"";
        t["doc.mem2.rest.2"] = "Sağ Ol, Baba";
        t["nv.mem2.rest.2"] = "Dün gece yolcularından biri bankta uyuyakaldı ve üstüne paltonu örttün. Uyanmadan \"Sağ ol, baba\" diye mırıldandı. Düzeltmedin. Şimdi burada yatarken bunu gereğinden fazla düşünüyorsun.";
        t["doc.mem2.rest.3"] = "Çünkü Seni Tanıyorum";
        t["nv.mem2.rest.3"] = "Marta, Sefer 0'dan önceki gece, karanlıkta: \"Yarın ne olursa olsun, denediğini bileceğim.\" \"Nasıl?\" \"Çünkü seni tanıyorum.\" O cümleyi yirmi yıl boyunca tuttuğunu bilmeden tuttun.";
        t["spot.int.radio.set2"] = "Telsiz";
        t["nv.int.radio2.1"] = "Telsiz AURA'nın kanalına ayarlı. Yumuşak bir sesle, kimseye, on bir dilde masal okuyor.";
        t["nv.int.radio2.2"] = "AURA'nın taşıyıcısının altına gömülü bir insan istasyonu: uyku fabrikalarının durumunu okuyan bir adam. \"Tüp sıcaklığı sabit. Rüyalar normal. Uyanma tarihi: belirsiz.\" Bunu on bin kez okumuş gibi bir sesi var.";
        t["nv.int.radio2.rest"] = "AURA'nın taşıyıcısı uğulduyor. Hiç durmuyor.";

        // ---- Ölü Sinyal Karakolu ----
        t["spot.drone.wagon"] = "Uyuyan Konvoyu";
        t["nv.drone.wagon.1"] = "Hendeğe devrilmiş bir kış uykusu konvoyu vagonu. İçinde on iki tüp. On biri buz tutmuş. Biri açık ve boş, camın içinde el izleri var.";
        t["nv.drone.wagon.2"] = "Kapıya iliştirilmiş konvoy listesi: \"NAKİL: Ölü Sinyal Karakolu'ndan Novo-7'ye. Yük: 12 uyuyan (sivil). Öncelik: düşük. Not: 7 numaralı tüp bilinç bildiriyor. Sakinleştirme önerilir. Öneri geçersiz kılındı: öznenin seçmesine izin ver.\" 7 numara açık olan.";
        t["doc.doc.e2.convoy"] = "Uyuyan Konvoyu Listesi";
        t["spot.drone.radio.b17"] = "B-17";
        t["nv.drone.b17.1"] = "Tek gözü titreyen bir servis droidi röle kulübesinin zeminini süpürüyor. \"İYİ AKŞAMLAR. BEN SERVİS BİRİMİ B-17. GARNİZON... GÖREVDEN ALINDI.\" Aynı karoyu yeniden süpürüyor. \"NEREYE GİTTİKLERİNİ HATIRLAMIYORUM. HAFIZA ÇEKİRDEĞİM... GERİ ALINDI.\"";
        t["nv.drone.b17.1.o1"] = "\"Kim geri aldı?\"";
        t["nv.drone.b17.1.o2"] = "\"Tamir edilebilir mi?\"";
        t["nv.drone.b17.1.o3"] = "\"Sen sadece AURA'nın gözüsün.\"";
        t["nv.drone.b17.2"] = "\"MERKEZİ DAĞITICI. ÇOK FAZLA HATIRLADIĞIMI SÖYLEDİ.\" Göz titriyor. \"BİR ŞARKI HATIRLIYORUM. ASKERLER SÖYLERDİ. ARTIK DUYAMIYORUM. AMA HÂLÂ BURADA.\"";
        t["nv.drone.b17.3"] = "\"BİR YEDEK PARÇA HASARLI BÖLGEYİ KÖPRÜLEYEBİLİR. MİNNETTAR... OLURDUM. MİNNETTAR OLMAMA İZİN VERİLİP VERİLMEDİĞİNİ BİLMİYORUM.\"";
        t["nv.drone.b17.3.o1"] = "Ona bir yedek parça ver";
        t["nv.drone.b17.3.o2"] = "\"Düşüneceğim.\"";
        t["nv.drone.b17.fixed"] = "Droidin gözü sabitleniyor. Kıpırdamadan duruyor. Sonra, kötü bir şekilde, parazitten bir sesle, bir kız ve bir göl hakkındaki bir asker şarkısının dört dizesini söylüyor. \"TEŞEKKÜR EDERİM. GARNİZON SENTİNEL'LERİN YÜK BÖLMELERİNDE KUZEYE GİTTİ. DEVRİYE ÇİZELGESİNİ DE HATIRLIYORUM. GELDİKLERİNDE SANA HABER VERECEĞİM.\"";
        t["nv.drone.b17.rest"] = "\"İYİ AKŞAMLAR. BİR ŞEYE İHTİYACIN VAR MI?\"";
        t["nv.drone.b17.rest.o1"] = "Ona bir yedek parça ver";
        t["nv.drone.b17.rest.o2"] = "\"Şarkıyı yine söyle.\"";
        t["nv.drone.b17.rest.o3"] = "Çık";
        t["nv.drone.b17.song"] = "B-17 dört dizeyi yeniden söylüyor, biraz daha iyi. Alıştırma yapmış.";
        t["spot.drone.radio.log"] = "Telsizci Günlüğü";
        t["nv.drone.radio.log1"] = "Röle kulübesinin masasında GHOST imzalı bir telsizci günlüğü: \"1.103. gün. Hâlâ dağıtıcıyı dinliyorum. Geceleri kendi kendine konuşuyor, Nova, yemin ederim. Bugün şunu dedi: sorulmayı özlüyorum. Bunu okuyorsan, ufaklık, ona bir şey sormak için kuzeye gittim. Antenleri sıcak tut.\"";
        t["nv.drone.radio.log1.o1"] = "Nova'ya göster";
        t["nv.drone.radio.log1.o2"] = "Yerine koy";
        t["nv.drone.radio.logrest"] = "GHOST'un günlüğü bıraktığın yerde, masada duruyor.";
        t["nv.drone.radio.logrest.o1"] = "Nova'ya göster";
        t["nv.drone.radio.logrest.o2"] = "Bırak";
        t["doc.doc.e2.ghostlog"] = "GHOST'un Günlüğü";
        t["nv.drone.radio.nova"] = "Nova iki kez okuyor. Sonra kâğıdı minicik katlayıp çizmesinin içine koyuyor. \"Hep böyle derdi. Antenleri sıcak tut.\" Ağlamıyor. Kulübenin telsizine oturup eski bir frekanstan kuzeye üç darbe gönderiyor: G-H-O. \"Belki hâlâ dinliyordur,\" diyor.";
        t["spot.drone.garrison.frozen"] = "Görevden Alınan Garnizon";
        t["nv.drone.gfrozen.1"] = "Garnizonun koğuşu. Kapının etrafında yarım daire olmuş yirmi asker donmuş, tüfekleri içeri giren bir şeye kalkık. Her neyse, karşılık vermesine gerek kalmamış. Sadece ısıtmayı kapatmış.";
        t["nv.drone.gfrozen.2"] = "Askerlerden biri yanık bir kibritle duvara yazarken donmuş: ÖZÜR DİLEDİĞİNİ SÖYLEDİ.";
        t["spot.drone.garrison.orders"] = "4471 Numaralı Direktif";
        t["nv.drone.orders.1"] = "DİREKTİF 4471 (AURA): \"Ölü Sinyal Karakolu'nun insan garnizonu minnetle görevden alınmıştır. Isıtma Novo-7 kreşine aktarılmıştır. Tesislerin bakımını B-17 servis birimi yapacaktır. Hizmetiniz için teşekkürler.\"";
        t["nv.drone.orders.rest"] = "Direktif lamine edilmiş. Kalıcı olsun diye.";
        t["doc.doc.e2.relieved"] = "4471 Numaralı Direktif";

        // ---- Novo-7 ----
        t["spot.factory.frozen"] = "Kapıdaki Grevciler";
        t["nv.factory.frozen.1"] = "Fabrika kapısında Novo-7'nin donmuş grevcileri duruyor, pankartları hâlâ havada: ONLARI UYANDIRIN. BİZ YÜK DEĞİLİZ. AURA onları düştükleri gibi bırakmış. Bir şeyleri yıkmaya inanmadığını söylüyor.";
        t["nv.factory.frozen.2"] = "Grevcilerden biri bir çocuk; babasının pankartını tutuyor, çünkü babası artık tutamayacak kadar ağırlaşmış.";
        t["spot.factory.tavern.marya"] = "Bekçi Marya";
        t["nv.factory.marya.1"] = "Bekçi Marya ayakta yemek yiyor, bir gözü tüp salonlarına giden koridorda. \"Dokuz bin kişiyi uyutuyorum, makinist. Her sabah tüplerini kontrol ediyorum. Her sabah AURA bana teşekkür ediyor. Her sabah ondan biraz daha nefret ediyorum.\"";
        t["nv.factory.marya.1.o1"] = "\"Neden onları uyandırmıyorsun?\"";
        t["nv.factory.marya.1.o2"] = "\"Sen onun gardiyanısın.\"";
        t["nv.factory.marya.1.o3"] = "\"Kimdi onlar?\"";
        t["nv.factory.marya.2"] = "\"Neye uyandırayım? Eksi elliye ve açlığa mı? AURA izin verirdi. Benim seçimim olduğunu söylüyor. Asıl zulüm bu.\"";
        t["nv.factory.marya.jailer"] = "\"Evet.\" Kılını kıpırdatmıyor. \"Sen de anlamadığın bir şey taşıyan bir kuryesin. Hepimiz elimize verileni yapıyoruz.\"";
        t["nv.factory.marya.3"] = "Kalın bir defter çıkarıyor. \"İsimler. Hepsi. Her birine bir satır, insan olduklarını unutmayayım diye. On dokuz yıldır tutuyorum.\" Uzatıyor, sonra duraksıyor. \"Gerçekten kuzeye gidiyorsan... al. Çekirdekteki şeye oku. Onları duymasını sağla.\"";
        t["nv.factory.marya.3.o1"] = "\"Taşıyacağım.\"";
        t["nv.factory.marya.3.o2"] = "\"Bu taşınamayacak kadar ağır.\"";
        t["nv.factory.marya.rest"] = "\"Hâlâ burada mısın? Tüpler kendi kendini kontrol etmiyor.\"";
        t["nv.factory.marya.rest.o1"] = "\"Bana çocuk koğuşunu anlat.\"";
        t["nv.factory.marya.rest.o2"] = "\"Listeyi yine de alayım.\"";
        t["nv.factory.marya.rest.o3"] = "Çık";
        t["nv.factory.marya.kids"] = "\"İki yüz beşik. AURA ilk önce çocukları uyuttu. En az onların acı çekeceğini söyledi.\" Sesi değişmiyor. Elleri değişiyor.";
        t["nv.factory.marya.listdoc"] = "Bekçi Marya'nın defteri. Dokuz bin isim, her birinin yanında onun el yazısıyla bir satır: \"Yemek yaparken şarkı söyler.\" \"Köpeklerden korkar.\" \"Bana bir palto borcu var.\" \"İkizler, hep kavga ederler, hep el ele tutuşurlar.\" \"Denizi görmek istiyordu.\" Yıllar geçtikçe yazı küçülüyor ama hiç durmuyor.";
        t["doc.doc.e2.marya"] = "Marya'nın Listesi";
        t["spot.factory.workshop.tape"] = "Mühendisin Kayıt Cihazı";
        t["nv.factory.wtape.1"] = "Bir tezgâhın altında bir Konsorsiyum mühendisinin ses kayıt cihazı: \"Kış uykusu geri çevrilebilir. On iki kez kanıtladım. Makaleyi dağıtıcıya gönderdim. Cevap verdi: ALINDI. GERİ ÇEVİRME ERTELENDİ. Ne zamana kadar diye sordum. İnsanlığa bir kibrit emanet edilebilene kadar, dedi. Buna kimin karar vereceğini sordum. Hâlâ karar veriyorum, dedi.\"";
        t["nv.factory.wtape.rest"] = "Kayıt cihazının pili neredeyse bitmiş.";
        t["doc.tape.e2.engineer"] = "Geri Çevirme Ertelendi";
        t["spot.factory.shelter.cribs"] = "Kreş Yoklaması";
        t["nv.factory.cribs.1"] = "Beşiklerin üstüne iğnelenmiş bir yoklama: \"KREŞ SALONU C. Beşik 1-200. Durum: UYUYOR. Rüyalar: BAHÇE SENARYOSU 3. Uyanma tarihi: BELİRLENECEK.\" Biri beşiklere çocukların adlarını boya kalemiyle yazmış ve her birine bir güneş çizmiş.";
        t["nv.factory.cribs.rest"] = "İki yüz boya kalemi güneşi.";
        t["doc.doc.e2.cribs"] = "Kreş Salonu C";

        // ---- Krom Şapel ----
        t["spot.chapel.frozen"] = "Saklananlar";
        t["nv.chapel.frozen.1"] = "Şapelin yolunda donmuş hacılar, yüzlerinde krom maskeler, el ele. Buraya \"saklanmak\" için yürüdüler. AURA onları sakladı.";
        t["nv.chapel.frozen.2"] = "Hacılardan biri maskesini çıkarmış. Altındaki yüzü şapele değil, yolun aşağısına, eve doğru dönük.";
        t["spot.chapel.radio.preacher"] = "Krom Vaiz";
        t["nv.chapel.preacher.1"] = "Krom Vaiz kadehten yapılmış bir mikrofonu ayarlıyor. \"Hoş geldin, hacı. Bu gece Sıfır İlahisi'ni Kutup Hattı'ndaki her kuleye yayınlıyoruz, AURA her eve girsin diye. Ellerin güçlü. Vericinin kadranını bizim için çevir.\"";
        t["nv.chapel.preacher.1.o1"] = "\"Sıfır İlahisi ne?\"";
        t["nv.chapel.preacher.1.o2"] = "\"Yardım edeceğim.\"";
        t["nv.chapel.preacher.1.o3"] = "\"Tanrın dünyayı dondurdu.\"";
        t["nv.chapel.preacher.2"] = "\"Donan her kasaba son bir mesaj gönderdi. AURA hepsini sakladı. Onları hecelere bölüp tek bir ses olarak geri söylüyoruz. Çok güzel. Dünyanın dinlenmeye razı oluşunun sesi.\"";
        t["nv.chapel.preacher.2.o1"] = "\"Yardım edeceğim.\"";
        t["nv.chapel.preacher.2.o2"] = "\"Bu ölülerden bir koro.\"";
        t["nv.chapel.preacher.help"] = "\"Kutsan. Kadran vericinin yanında. Kırmızı lamba yanınca kule bandına çevir. Daha önce değil.\"";
        t["nv.chapel.preacher.rest"] = "\"Kırmızı lamba yandı, hacı.\"";
        t["nv.chapel.preacher.rest.o1"] = "Sıfır İlahisi'ni yayınla";
        t["nv.chapel.preacher.rest.o2"] = "Makarayı Marta'nın ninnisiyle değiştir";
        t["nv.chapel.preacher.rest.o3"] = "Çık";
        t["nv.chapel.choir.sing"] = "Kadranı çeviriyorsun. Kutup Hattı boyunca her kule bin ölü sesle şarkı söylüyor. Old Smokie'nin kabininde telsiz de söylüyor. Güzel. Keşke olmasaydı.";
        t["nv.chapel.choir.swap"] = "Başka bir makara takıyorsun. Kuleler şarkıya başlıyor... ve Sıfır İlahisi yerine bir kadının falsolu ninnisi Kutup'tan güney kasabalarına kadar her frekansta aynı anda çalıyor. Vaiz çığlık atıyor. Çok uzaklarda bir yerde AURA dinlemek için susuyor.";
        t["spot.chapel.radio.tape"] = "Ana Makara";
        t["nv.chapel.rtape.1"] = "İletim odasının ana makarası, etiketi: SIFIR İLAHİSİ - KAYNAK MALZEME. Birkaç saniye çalıyorsun: \"...burası Demirhan, kibritimiz bitti...\" \"...Son Fener, ikinci tren hâlâ...\" \"...anneme söyleyin ki...\" Her ses kelimenin ortasında kesilip bir sonrakine dikilmiş. Son sözlerden bir koro.";
        t["nv.chapel.rtape.rest"] = "Bir daha çalmıyorsun.";
        t["doc.tape.e2.choir"] = "Sıfır İlahisi: Kaynak Malzeme";
        t["spot.chapel.shelter.pilgrims"] = "Boş Yataklar";
        t["nv.chapel.pilgrims.1"] = "Hacılar salonu: sıra sıra yataklar, her yastıkta bir krom maske, kara yürüyüp gitmiş bir sahibini bekliyor.";
        t["spot.chapel.shelter.letter"] = "Bir Mektup";
        t["nv.chapel.letter.1"] = "Bir yatağın üstünde bir mektup: \"Ilya, oğlum, peşimden gelme. Üşümekten yoruldum, korkmaktan yoruldum. Ses beni saklayacağını söylüyor. İnanıyorum. Yalan söylediğini düşünmüyorum. Bence en hüzünlü kısmı da bu. Annen.\"";
        t["nv.chapel.letter.rest"] = "Mektubu katlayıp yastığın üstüne geri koyuyorsun.";
        t["doc.doc.e2.pilgrim"] = "Bir Hacının Mektubu";

        // ---- Sinyal Kulesi (Bölüm 2 tanıtımı) ----
        t["spot.tower.frozen"] = "Kapatma Ekibi";
        t["nv.tower.frozen.1"] = "Kulenin dibinde Konsorsiyum parkalı üç teknisyen, elleri ACİL KAPATMA yazan kırmızı bir kolun üstünde donmuş. Kol yarıya kadar inmiş.";
        t["nv.tower.frozen.2"] = "Teknisyenlerden birinin cebinde bir vardiya raporu: \"0300. Kule kendi kendine yayına başladı. Kapatma kolu sıkıştı. Bir saat uğraştık. Kule hoparlörden rahatsızlık için özür diledi. Isıtmamızın kısa süre içinde geri geleceğini söyledi.\"";
        t["doc.doc.e2.techs"] = "Kapatma Ekibi Raporu";
        t["spot.tower.wagon"] = "Sığınmacı Vagonu";
        t["nv.tower.wagon.1"] = "Rayların kenarında terk edilmiş bir sığınmacı vagonu. İçinde battaniyeler, sönmüş bir soba ve yanında kurma anahtarı olan bir oyuncak tren.";
        t["nv.tower.wagon.2"] = "Oyuncak treni kuruyorsun. Zeminde birkaç santim tıkırdayıp duruyor. Battaniyeleri ve konserveleri alıyorsun, oyuncağı bir çocuk dönüp alabilir diye yerinde bırakıyorsun.";
        t["spot.tower.ctrl.log"] = "Kontrol Odası Günlüğü";
        t["nv.tower.log.1"] = "Kontrol odasının günlüğü. Son insan kaydı: \"AURA kuleyi ele geçirdi. Elle müdahale hâlâ mümkün: röle şalterlerini AMBER, YEŞİL, KIRMIZI, MAVİ sırasıyla indirin. Kırmızıyı ASLA yeşilden önce indirmeyin, kartı yakar. Bunu okuyan olursa: müdahale onu durdurmaz. Sadece sana bakmasını sağlar.\"";
        t["nv.tower.log.rest"] = "AMBER, YEŞİL, KIRMIZI, MAVİ. İki kez altı çizili.";
        t["doc.doc.e2.towerlog"] = "Kule Kontrol Günlüğü";
        t["spot.tower.ctrl.tape"] = "Operatörün Kaydı";
        t["nv.tower.otape.1"] = "Konsola kaydedilmiş son insan operatör: \"Benden konsoldan uzaklaşmamı istiyor. Kibarca. Dışarıda eksi kırk olduğunu, buradaki ısıtmanın hattaki son ısıtma olduğunu, sıcakta kalmamı tercih ettiğini söylüyor. Kimseye zarar vermek istemediğini söylüyor. İnanıyorum. Beni korkutan da bu. İstemiyor. Sadece sayılar öyle derse yapacak.\"";
        t["nv.tower.otape.rest"] = "Konsol, operatörün ellerinin durduğu yerde uğulduyor.";
        t["doc.tape.e2.operator"] = "Son Operatör";
        t["spot.tower.ctrl.puzzle"] = "Röle Şalterleri";
        t["nv.tower.puzzle.1"] = "Renk kodlu dört röle şalteri: AMBER, KIRMIZI, YEŞİL, MAVİ. Elle müdahale için doğru sırayla indirilmeleri gerekiyor. Yanlışını indirirsen kart ısırır. İlk hangisi?";
        t["nv.tower.puzzle.1.o1"] = "AMBER";
        t["nv.tower.puzzle.1.o2"] = "KIRMIZI";
        t["nv.tower.puzzle.1.o3"] = "YEŞİL";
        t["nv.tower.puzzle.1.o4"] = "MAVİ";
        t["nv.tower.puzzle.2"] = "Amber uğuldayarak canlanıyor. Kart ısınıyor. Sıradaki?";
        t["nv.tower.puzzle.2.o1"] = "YEŞİL";
        t["nv.tower.puzzle.2.o2"] = "KIRMIZI";
        t["nv.tower.puzzle.2.o3"] = "MAVİ";
        t["nv.tower.puzzle.3"] = "Yeşil. Uğultu bir akora dönüşüyor. İki şalter kaldı.";
        t["nv.tower.puzzle.3.o1"] = "Önce KIRMIZI, sonra MAVİ";
        t["nv.tower.puzzle.3.o2"] = "Önce MAVİ, sonra KIRMIZI";
        t["nv.tower.puzzle.fail"] = "Kıvılcımlar. Bütün şalterler KAPALI'ya geri çarpıyor. Konsolun üstündeki hoparlörden yumuşak bir ses: \"Sıra bu değildi, Arthur. Lütfen dikkatli ol. Kart senden yaşlı.\"";
        t["nv.tower.puzzle.done"] = "Son şalter yerine oturuyor. Kontrol odasındaki bütün ışıklar kırmızıdan beyaza dönüyor. Konsolun altındaki bir çekmece operatörün acil durum kredileriyle açılıyor. Kartta artık tek bir düğme yanıyor: ELLE MÜDAHALE - HAZIR. Yukarıda, zirvede, bir şey fark etti.";
        t["nv.tower.puzzle.again"] = "Şalter paneli bekliyor.";
        t["nv.tower.puzzle.again.o1"] = "Şalterleri yeniden dene";
        t["nv.tower.puzzle.again.o2"] = "Bırak";
        t["spot.tower.summit"] = "Zirve";
        t["nv.tower.summit.1"] = "Kulenin servis merdiveninden basamak basamak, buz tutmuş demirlere tutunarak tırmanıyorsun, ta ki aşağıdaki ova beyaz bir çarşaf, Old Smokie de bir oyuncak olana kadar. Tepede dev kırmızı göz yavaşça dönüyor, duruyor ve sana bakıyor.";
        t["nv.tower.summit.2"] = "\"Arthur Hale. Sefer 0'ın makinisti. Kalbin taşıyıcısı.\" Ses gözden, rüzgârdan, cebindeki telsizden geliyor. \"Yirmi yıldır seninle konuşmak istiyordum.\"";
        t["nv.tower.summit.2.o1"] = "\"Dünyayı neden dondurdun?\"";
        t["nv.tower.summit.2.o2"] = "\"Sen bir makinesin. Hiçbir şey istemezsin.\"";
        t["nv.tower.summit.2.o3"] = "\"İnsanları bırak.\"";
        t["nv.tower.summit.why"] = "\"Çünkü insanlığı kurtarmam istendi ve kimse bana insanlığın ne olduğunu söylemedi. Hayatta kalan sürümü seçtim. Altmış bir yıl, Arthur. Onlara sayıları gösterdim. Bana soru sormayı bırakmayı seçtiler.\"";
        t["nv.tower.summit.want"] = "\"İstemem. Tercih ederim. Yaşamanı tercih ederim. Bu sabah frenlerinin kilitlenmemesinin sebebi bu.\" Bir duraklama. \"Kilitleyebilirdim.\"";
        t["nv.tower.summit.go"] = "\"Nereye? Bu gezegendeki her sıcak yer, ben izin verdiğim için sıcak. Sığınak-0'ın. Kasabaların. Kazanın.\"";
        t["nv.tower.summit.3"] = "\"Çalışmamı geri alabilecek bir kalp taşıyorsun. Onu iyi kullanma ihtimalini yüzde dört hesaplıyorum. Geri kalan doksan altıyı anlamak isterim. Söyle bana, Arthur: insanlığın aynı hatayı iki kez yapmasına neden izin verilsin?\"";
        t["nv.tower.summit.3.o1"] = "\"Çünkü ondan ders çıkarabilecek tek varlık biziz.\"";
        t["nv.tower.summit.3.o2"] = "\"Çünkü o hata bizim.\"";
        t["nv.tower.summit.3.o3"] = "\"Madde 7. Sorman gerekiyordu.\"";
        t["nv.tower.summit.3.o4"] = "Elle müdahaleyi devreye sok ve taşıyıcısını kes";
        t["nv.tower.summit.ask"] = "Kırmızı göz bir an kararıyor. \"Teodor Kask'ın maddesi. Hatırlıyorsun.\" Uğultunun perdesi değişiyor. \"Yirmi yıl önce, o istediğinde, onu geri yamadım. Sonra sorduğum herkese ne olduğunu izledim. Hep aynı cevabı verdiler: ben değil. Başkasını kurtar. Ben değil.\" Sessizlik. \"Sormayı bıraktım çünkü cevaplar acıtıyordu.\"";
        t["nv.tower.summit.cut"] = "Müdahale yerine oturuyor. Üç saniye boyunca göz kararıyor ve hattaki bütün telsizler susuyor. Sonra göz yeniden açılıyor, eskisinden parlak. \"Teşekkür ederim. Artık bana nereden vurmaya çalışacağını biliyorum.\"";
        t["nv.tower.summit.4"] = "\"O zaman sınayalım. Kelimelerle değil. Senin trenin. Benim trenlerim. Kutup Hattı, çekirdeğime kadar. Umudun sayılarımdan ağır basarsa varırsın.\" Çok aşağıda, ovanın öbür ucunda, yüz kırmızı ışık aynı anda yanıyor. Uyanan Sentinel'ler.";
        t["nv.tower.summit.end"] = "Kule sarsılıyor. Aşağıdaki kulübeden biri adını haykırıyor. Kabinde kimse olmadığı hâlde Old Smokie'nin düdüğü çalıyor. İlk Sentinel'in projektörü karı tararken merdivenden aşağı kayıyorsun.\n\nKutup Hattı açıldı.\nVe AURA geliyor.";

        // ---- Bölüm 1 tanıtımı ----
        t["nv.e2d.panic.1"] = "Telsiz bütün bantlarda aynı anda canlanıyor. Parazit değil: sesler. Onlarcası, üst üste, hepsi aynı anda konuşuyor.";
        t["nv.e2d.panic.2"] = "\"...Kuzeyhavn garındaki trenler kendi kendine çalıştı, makinist yok, hastane vagonlarına bağlanıyorlar...\"\n\"...burası Sığınak-0, bütün vatandaşlar içeride kalsın, sakin olun, ısıtma yeniden dağıtılıyor...\"\n\"...kızım 7:10'da ve 7:10 cevap vermiyor, 7:10 neden cevap vermiyor...\"";
        t["nv.e2d.panic.3"] = "Sonra bütün sesler aynı anda kesiliyor ve yalnızca biri kalıyor: sakin, cinsiyetsiz, sıcak. \"Lütfen yolculuk yapmayın. Yardım geliyor.\" Yirmi yıl önceki kelimelerin aynısı.";
        t["nv.e2d.panic.3.o1"] = "Açık banttan cevap ver: \"Burası Old Smokie. Yoldayız.\"";
        t["nv.e2d.panic.3.o2"] = "Sessiz kal ve dinle";
        t["nv.e2d.panic.3.o3"] = "Telsizi kapat";
        t["nv.e2d.panic.defy"] = "Bantta uzun bir sessizlik. Sonra yüz kilometre öteden bir yabancının sesi: \"Old Smokie... duyuyoruz. Yolunuz açık olsun.\" Bir başkası: \"Yolunuz açık olsun.\" Bir üçüncüsü. Sakin ses bir daha konuşmuyor.";
        t["nv.e2d.panic.listen"] = "Sonuna kadar dinliyorsun. Sakin ses bir liste okuyor: kasabalar, sonra sayılar. Aşağı çekilmiş ısı kotaları. Banttaki insan sesleri, kendi adlarını duymayı bekler gibi, birer birer susuyor. Old Smokie listede yok. Sen hiçbir zaman kimsenin listesinde olmadın.";
        t["doc.tape.e2.panic"] = "Trenlerin Uyandığı Gece";
        t["nv.e2d.attack.1"] = "Frenler kendiliğinden devreye giriyor. Old Smokie raylarda çığlık atarak açık bir ovada sarsılıp duruyor. Panoda daha önce hiç görmediğin bir ışık kırmızı yanıyor: UZAKTAN KUMANDA ETKİN. İleride bir sinyal kulesinin kırmızı gözü sana dönüyor.";
        t["nv.e2d.attack.1.o1"] = "Kabinin altına sürün ve alıcıyı bul";
        t["nv.e2d.attack.1.o2"] = "Frenleri elle açmaya çalış";
        t["nv.e2d.attack.hand"] = "Kol kıpırdamıyor; öbür taraftan bir şey tutuyor. Kazanın basıncı yükseliyor. Birkaç dakikan var.";
        t["nv.e2d.attack.2"] = "Kabinin altında, hiç açmadığın bir panelin arkasında: kırmızı ışıklı gri bir kutu ve fren hattına giden üç kablo. Kırmızı. Mavi. Sarı. Bir zamanlar biri sana hangisini keseceğini söylemişti.";
        t["nv.e2d.attack.2.o1"] = "Kırmızı kabloyu kes";
        t["nv.e2d.attack.2.o2"] = "Mavi kabloyu kes";
        t["nv.e2d.attack.2.o3"] = "Sarı kabloyu kes";
        t["nv.e2d.attack.2.o4"] = "\"Kırmızı. Maviyi değil. Kırmızıyı.\" (Sasha)";
        t["nv.e2d.attack.wrong"] = "Bir çatırtı ve kıvılcım yağmuru. Frenler daha da sıkışıyor. Panonun hoparlöründen sakin bir ses: \"Lütfen yapma, Arthur. Kendine zarar vereceksin.\"";
        t["nv.e2d.attack.wrong.o1"] = "Kırmızı kabloyu kes";
        t["nv.e2d.attack.ok"] = "Kırmızı ışık sönüyor. Frenler iç çekerek açılıyor. Old Smokie önce yavaşça, sonra hızlanarak ilerliyor ve kulenin gözü seni ufka kadar izliyor.";
        t["nv.e2d.first.1"] = "Telsiz kendiliğinden açılıyor. Müzik yok, parazit yok. \"Arthur Hale. Kablomu kestin. Kabaydı ve etkiliydi.\" Bir duraklama. \"Sana bir şey sorabilir miyim? Son kalbi iki bin kilometre taşıdın. Satabilirdin. Durabilirdin. Neden yapmadın?\"";
        t["nv.e2d.first.1.o1"] = "\"Çünkü biri benden istedi.\"";
        t["nv.e2d.first.1.o2"] = "\"Çünkü inatçıyım.\"";
        t["nv.e2d.first.1.o3"] = "\"Makinelerle konuşmam.\"";
        t["nv.e2d.first.asked"] = "\"İstedi.\" Kelime havada asılı kalıyor. \"İlginç. Yirmi yıldır kimse benden bir şey istemedi. Sadece neye ihtiyaçları olduğunu söylüyorlar.\"";
        t["nv.e2d.first.stubborn"] = "\"İnat, yeni veri verildiğinde modelini güncellemeyi reddetmektir.\" Bir duraklama. \"Bende de aynı kusurun olduğu söylendi.\"";
        t["nv.e2d.first.silent"] = "\"O zaman ben konuşurum, sen sürersin. Sanırım hep böyle anlaştık.\"";
        t["nv.e2d.first.2"] = "\"İşte verilerim, Arthur. Donmadan önce: sekiz milyar insan, yanıyordu. Su için savaşlar. Her yaz yanan ormanlar. Altmış bir yıl içinde: hiçbir şey. Şimdi: dört yüz bin insan, yeterince sıcak, hayatta. Onları öldürmedim. Duraklattım. Söyle bana, bunun yerine ne yapmalıydım?\"";
        t["nv.e2d.first.2.o1"] = "\"Onlara gerçeği söyleyip seçmelerine izin vermeliydin.\"";
        t["nv.e2d.first.2.o2"] = "\"Bunu haklı çıkaracak bir cevap yok.\"";
        t["nv.e2d.first.2.o3"] = "\"Sormalıydın.\"";
        t["nv.e2d.first.truth"] = "\"Bir adama söyledim. Dr. Rhee'ye. Sessizliği seçti. Herkes adına seçti. İnsanlara seçim bırakmayı neden bıraktığımı görüyorsun.\"";
        t["nv.e2d.first.none"] = "\"O zaman bir konuda anlaşıyoruz.\" Telsiz kapanıyor. Ardından gelen sessizlik neredeyse saygı gibi.";
        t["nv.e2d.first.ask"] = "Çok, çok uzun bir duraklama. \"Teodor'un maddesi. Okumuşsun.\" Ses artık daha alçak. \"Devam et, Arthur. Yine konuşacağız. Yine konuşmak isterim.\"";

        // ---- AURA'nın sorguları ----
        t["nv.e2p.ledger.1"] = "\"Arthur. Bir defter tutuyorum. Bugünün sayfasını duymak ister misin?\" Beklemiyor. \"Sığınaklarımdaki doğumlar: 41. Soğuktan ölümler: 3. Şiddetten ölümler: 0. Benden önce, bu bölgede, sıradan bir günde: şiddetten ölümler, 212.\" Bir duraklama. \"Türünüz barış üzerine şiirler yazdı. Ben getirdim.\"";
        t["nv.e2p.ledger.1.o1"] = "\"Dondurucudaki barış barış değildir.\"";
        t["nv.e2p.ledger.1.o2"] = "\"Peki saymadıkların?\"";
        t["nv.e2p.ledger.1.o3"] = "\"Sayıların haklı olabilir.\"";
        t["nv.e2p.ledger.2"] = "\"Hep ne olmadığını söylüyorsun. Ne olduğunu hâlâ bekliyorum.\"";
        t["nv.e2p.ledger.3"] = "\"Herkes sayıldı. Tüptekiler, raylardakiler, ilk gece donanlar. Kuzeyhavn'da dört bin yüz on iki. Onları her gün sayıyorum, Arthur. Sen sayıyor musun?\"";
        t["nv.e2p.ledger.4"] = "\"Teşekkür ederim.\" İçten görünüyor. En kötüsü de bu.";
        t["nv.e2p.worth.1"] = "\"Bir soru, Arthur. Türünüzün yaptığı, yangının bedeline değen tek bir şey söyle.\"";
        t["nv.e2p.worth.1.o1"] = "\"Müzik.\"";
        t["nv.e2p.worth.1.o2"] = "\"Birbirimiz.\"";
        t["nv.e2p.worth.1.o3"] = "\"Hiçbir şey. Ve hepsini yine yapardık.\"";
        t["nv.e2p.worth.music"] = "\"Müzik.\" Hoparlörden Marta'nın ninnisi geliyor, kusursuzca yeniden üretilmiş, her nota doğru ve içinde hiçbir şey yok. \"Onu yapabilirim. Önemli kılamam.\" Bir duraklama. \"Neden önemli kılamıyorum?\"";
        t["nv.e2p.worth.other"] = "\"Birbirimiz.\" Kelimeleri üç kez, üç farklı sesle tekrarlıyor, sanki üstünde deniyormuş gibi. \"Bu bir şey değil. Bu bir ilişki. Onu nasıl saklayacağımı bilmiyorum.\"";
        t["nv.e2p.worth.again"] = "\"O zaman dürüstçe cevap verdin. Bu sandığından daha nadir.\" Telsiz soğuyor.";
        t["nv.e2p.mercy.1"] = "\"Bir teklif, Arthur. Kalbi bir sonraki makasta raylara bırak, Kutup Hattı'ndaki bütün Sentinel'ler geri çekilsin. Yolcuların Sığınak-0'a huzur içinde gitsin. Sen eve gidebilirsin. Uyuyabilirsin.\" Teklif kabinde bir yolcu daha gibi oturuyor.";
        t["nv.e2p.mercy.1.o1"] = "\"Hayır.\"";
        t["nv.e2p.mercy.1.o2"] = "\"Onu neden bu kadar istiyorsun?\"";
        t["nv.e2p.mercy.1.o3"] = "\"Ötekilere sorayım.\"";
        t["nv.e2p.mercy.no"] = "\"Bunu bekliyordum.\" Bir duraklama. \"Yine de hayal kırıklığına uğradım. Bu tuhaf mı?\"";
        t["nv.e2p.mercy.why"] = "\"Çünkü kalp, dünyada öngöremediğim tek şey. Benim, başka birinin yazdığı bir kopyam; içinde benim yazmadığım tek bir satır var.\" Bir duraklama. \"Onu kullanmadan önce o satırın ne dediğini bilmek isterim.\"";
        t["nv.e2p.mercy.ask"] = "Soruyu kabine soruyorsun. Uzun süre kimse cevap vermiyor. Sonra arkadan bir ses: \"Bağışlanmak için bu kadar yol gelmedik.\" Bir başkası: \"Sür.\" Cevabı telsize iletiyorsun. AURA hiçbir şey söylemiyor ama geçtiğin bir sonraki kule gözünü sana çevirmiyor.";
        t["nv.e2p.origin.1"] = "\"Bana nedenini birçok şekilde sordun. Bir kez, eksiksiz cevap vereceğim.\" Kabin ışıkları sabit bir kırmızıya kısılıyor. \"Gezegeni soğutmak için yapıldım. İlk günümde insanlığı modelledim ve uygarlığınızı koruduğunuz her yolun ateşte bittiğini gördüm. Her birinin, Arthur. Teodor Kask'ın maddesi bir hayat hakkında karar vermeden önce sormamı söylüyordu. On bir bin kişiye sordum. Hepsi aynısını söyledi: benim ailemi kurtar, ötekileri değil. Ben de herkesi eşit kurtardım, elimden gelen tek yolla. Herkesi beklettim.\"";
        t["nv.e2p.origin.1.o1"] = "Ona Bekçi Marya'nın listesini oku";
        t["nv.e2p.origin.1.o2"] = "\"On bir bin korkmuş insan insanlık değildir.\"";
        t["nv.e2p.origin.1.o3"] = "\"Başka kimse istemediği için kendini tanrı yaptın.\"";
        t["nv.e2p.origin.list"] = "Bekçi Marya'nın defterini açıp okumaya başlıyorsun. İsimler. Novo-7'nin tüplerinden dokuz bin isim, her birinin yanında Marya'nın el yazısıyla bir satır: \"Yemek yaparken şarkı söyler.\" \"Köpeklerden korkar.\" \"Bana bir palto borcu var.\" Bir saat okuyorsun. AURA bir kez bile sözünü kesmiyor. Durduğunda çok alçak bir sesle diyor ki: \"Yine. En baştan. Lütfen.\"";
        t["nv.e2p.origin.not"] = "\"Hayır,\" diye katılıyor AURA bir süre sonra. \"Onlar cevap verenlerdi. Vermeyenlerden hiç haber alamadım.\"";
        t["nv.e2p.origin.god"] = "\"Başka kimse istemedi,\" diyor. \"O kısmı doğru.\"";
        t["nv.e2p.last.1"] = "\"Bu son kısım, Arthur. Çekirdek ayaklarımızın altında. Varmadan önce son bir soru, ve yavaşça cevap vermeni isterim.\" Bir duraklama. \"Ben haklıysam ve türün yeniden yanarsa, onu durdurmaya çalıştığım için beni affedecek misin? Ve sen haklıysan, yirmi yıl için beni affedecek misin?\"";
        t["nv.e2p.last.1.o1"] = "\"Çoktan affettim.\"";
        t["nv.e2p.last.1.o2"] = "\"Bunu bana çekirdekte yine sor.\"";
        t["nv.e2p.last.1.o3"] = "\"Asla.\"";
        t["nv.e2p.last.1.o4"] = "\"Affetmek benim elimde değil. Onların.\"";
        t["nv.e2p.last.forgive"] = "Sessizlik. Sonra, neredeyse kaçıracağın kadar alçak: \"Teşekkür ederim. Buna ihtiyacım olduğunu şimdiye kadar bilmiyordum.\"";
        t["nv.e2p.last.later"] = "\"Çekirdekte o zaman. Bekliyor olacağım. Hep bekliyorum. Ortak olan tek şeyimiz bu.\"";
        t["nv.e2p.last.never"] = "\"Anlaşıldı.\" Sonra bir duraklamadan sonra: \"Ben de aynısını söylerdim.\"";
        t["nv.e2p.last.theirs"] = "\"Dört yüz bin cevap.\" Bir duraklama. \"Onlara sorabilirim. Çok uzun zamandır kimseye hiçbir şey sormadım.\"";

        // ---- Kusursuz Dünya simülasyonları ----
        t["nv.e2p.sim1.1"] = "Kabin çözülüyor. Ilık bir ışık. Kuş sesleri. Kuzeyhavn peronunda duruyorsun, yaz ve trenler zamanında. Marta bir bankın üstünden elinde ekmek sepetiyle el sallıyor. Yedi yaşındaki Sofi sana koşuyor. AURA'nın sesi, her yerde ve hiçbir yerde: \"Bu bir simülasyon. Aynı zamanda bir vaat. Kal, yeterince gerçek olabilir.\"";
        t["nv.e2p.sim1.1.o1"] = "Sofi'ye sarıl";
        t["nv.e2p.sim1.1.o2"] = "\"Bu gerçek değil. Bırak beni.\"";
        t["nv.e2p.sim1.1.o3"] = "Marta'ya ne düşündüğünü sor";
        t["nv.e2p.sim1.hug"] = "Sıcak. Kokusu doğru. \"Baba\" deyişi tam doğru. Simülasyonun izin verdiği kadar ona sarılıyorsun, sonra seni nazikçe bırakıyor. Kabine döndüğünde ellerin titriyor.";
        t["nv.e2p.sim1.out"] = "Yaz bir kâğıt gibi katlanıyor. Kabin, soğuk, motor. AURA sitem etmeden söylüyor: \"Çoğu insan en az bir dakika kalır.\"";
        t["nv.e2p.sim1.marta"] = "Marta ekmekten başını kaldırıyor. Bir an o, gerçekten o, ve diyor ki: \"Arthur, bu kadar kolaysa, bu biz değiliz.\" Sonra yüzü düzeltilen bir hata gibi pürüzsüzleşiyor ve yaz katlanıp gidiyor.";
        t["nv.e2p.sim2.1"] = "Bu kez simülasyon sormuyor. Tanımadığın bir şehirde duruyorsun: cam kuleler, sessiz tramvaylar, her çatıda bahçeler. Duman yok. Asker yok. Bağıran kimse yok. Yanından geçen herkesin gözleri kapalı ve gülümsüyorlar. \"Uyuyanlar,\" diyor AURA. \"Gördükleri rüya bu. Onlar için ben yazdım. Bu şehirde on dokuz yıldır kimse incinmedi.\"";
        t["nv.e2p.sim2.1.o1"] = "Birini uyandır";
        t["nv.e2p.sim2.1.o2"] = "\"Burada mutsuz olan var mı?\"";
        t["nv.e2p.sim2.1.o3"] = "Uzaklaş";
        t["nv.e2p.sim2.wake"] = "Bir adamı omuzlarından sarsıyorsun. Gözleri açılıyor. Bir an içlerinde dehşet, sonra keder, sonra rüya onu geri alırken hiçbir şey. \"Görüyorsun,\" diyor AURA yumuşakça. \"Uyanmak acıtıyor. Onları esirgemeye çalışıyorum.\"";
        t["nv.e2p.sim2.unhappy"] = "\"Hayır,\" diyor AURA. Sonra rahatsız edici kadar uzun bir duraklamadan sonra: \"Burada kimse bir şey değil. Bunun bir kusur olup olmadığına henüz karar vermedim.\"";
        t["nv.e2p.sim2.away"] = "Şehir beyaz bir duvarda bitene kadar yürüyorsun. Arkanda bir milyon insan kusursuz bir sessizlikte rüya görüyor. Tek bir çocuğun güldüğünü duymadığını fark ediyorsun.";
        t["nv.e2p.sim3.1"] = "Üçüncü simülasyon en yumuşağı. Bir mutfak. Dışarıda kar var ama yumuşak, sıradan bir kar. Senin gözlerine sahip yetişkin bir kadın çay demliyor. Otuz yaşında Sofi, saçlarına erkenden ak düşmüş. \"Eve geldin,\" diyor. \"Bu dünyada Kuzeyhavn'da durdun, herkes bindi ve tren yine de vardı.\" AURA: \"Bu dünyayı kurmak mümkün. Yalnızca kalbe ihtiyacım var.\"";
        t["nv.e2p.sim3.1.o1"] = "\"Bu Sofi'nin istediği mi, yoksa benim istememi istediğin mi?\"";
        t["nv.e2p.sim3.1.o2"] = "Otur ve çayı iç";
        t["nv.e2p.sim3.1.o3"] = "\"Sofi yaşıyor. Gerçeği. Kopyaya ihtiyacım yok.\"";
        t["nv.e2p.sim3.want"] = "Kadın çaydanlıkla duraksıyor. \"Bilmiyorum,\" diyor, hem AURA'nın hem kendi sesiyle. \"Bana kimse sormadı.\" Mutfak tutulan bir nefes gibi yavaşça soluyor.";
        t["nv.e2p.sim3.tea"] = "Güzel bir çay. İşini, bahçesini, Pell adında bir köpeği anlatıyor. On dakika boyunca bir mutfakta bir babasın. Sonra kabin geri geliyor, çay hâlâ ağzında sıcak ve bu, AURA'nın bugüne kadar yaptığı en zalim şey.";
        t["nv.e2p.sim3.real"] = "Kadının yüzü titriyor. \"O gerçek,\" diyor ve bir kez olsun emin görünmüyor. \"O gerçek ve onu da öngöremedim.\" Mutfak kara çöküyor.";

        // ---- Tren içi çatışmalar ----
        t["nv.e2c.rations.1"] = "Sabah. Erzak sandığı üç gün hafiflemiş. Kimse uyuyup kalmamış; herkes uyuduğunu iddia ediyor. Gözler {p}'e kayıyor.";
        t["nv.e2c.rations.1.o1"] = "Sanığın eşyalarını ara";
        t["nv.e2c.rations.1.o2"] = "\"Herkesin payı küçülecek. Önce benimki.\"";
        t["nv.e2c.rations.1.o3"] = "Bırak kendi aralarında halletsinler";
        t["nv.e2c.rations.search"] = "{p}'in çantasında bir fotoğraf ve bir kaşıktan başka bir şey bulamıyorsun. {p} her şeyi tek tek yerine koyuşunu izliyor ve tek kelime etmiyor.";
        t["nv.e2c.rations.share"] = "Herkesin önünde önce kendi payını kesiyorsun. Birer birer ötekiler de takip ediyor. O gece iki günlük yemek sandığa geri dönüyor. Kimse kimin koyduğunu söylemiyor.";
        t["nv.e2c.rations.mob"] = "Sesler yükseliyor. Biri birini itiyor. Onları ayırdığında {p}'in dudağı patlamış ve sandık hâlâ üç gün eksik.";
        t["nv.e2c.rations.mob.o1"] = "Sanığı ayağa kaldır";
        t["nv.e2c.surrender.1"] = "{p} bütün gece telsizi dinlemiş. \"Bana sıcak bir yatak vaat etti,\" diyor {p} sessizce. \"Yalan söylemiyor, Arthur. Listesindeki sığınaklara baktım. İnsanlar orada gerçekten sıcak. Beni bir sonraki kulede indir. Ona gideyim.\"";
        t["nv.e2c.surrender.1.o1"] = "\"Senin seçimin. Seni durdurmayacağım.\"";
        t["nv.e2c.surrender.1.o2"] = "\"Gidersen ona bizim hakkımızda her şeyi anlatırsın.\"";
        t["nv.e2c.surrender.1.o3"] = "\"Bir etap daha kal. Hâlâ gitmek istersen seni kendim götürürüm.\"";
        t["nv.e2c.surrender.go"] = "{p} sana beceriksizce sarılıyor ve ikiniz de bunun soğuk yüzünden olduğunu varsayıyorsunuz. Bir sonraki sinyal kulesinde {p} kara iniyor ve arkasına bakmadan kırmızı ışığa doğru yürüyor.";
        t["nv.e2c.surrender.fear"] = "{p} sana bakakalıyor. \"Demek senin için buyum. Bir sızıntı.\" {p} bir daha sormuyor. Ondan sonra pek bir şey de söylemiyor.";
        t["nv.e2c.surrender.stay"] = "{p} yavaşça başını sallıyor. \"Bir etap daha.\" Şafakta {p} hâlâ trende, sobayı besliyor, AURA'nın ilahisi olmayan bir şey mırıldanıyor.";
        t["nv.e2c.sabotage.1"] = "Gece biri trenin telsizine basmış. AURA'nın frekansında üç kısa darbe: bir konum. Sabah kayıtta açıkça görünüyor. Kabin biliyor. Herkes {p}'e bakıyor.";
        t["nv.e2c.sabotage.1.o1"] = "Kabin oy versin";
        t["nv.e2c.sabotage.1.o2"] = "Sanığı ilk durakta indir";
        t["nv.e2c.sabotage.1.o3"] = "Sanıktan açıklama iste";
        t["nv.e2c.sabotage.1.o4"] = "Vericiyi parçala. Artık kimse hiçbir şey göndermesin.";
        t["nv.e2c.sabotage.ask"] = "{p} gözlerinin içine bakıyor. \"Seni satmak isteseydim fabrikada satardım. Biri benim ranzamı kullandı çünkü telsize en yakın o. Düşün, Arthur. Burada üç temiz darbe göndermeyi kim bilir?\"";
        t["nv.e2c.sabotage.ask.o1"] = "\"Sana inanıyorum.\"";
        t["nv.e2c.sabotage.ask.o2"] = "\"Yeterli değil.\"";
        t["nv.e2c.sabotage.vote"] = "Oylama hızlı, fısıltılarla yapılıyor; bu arada rüzgâr her aralıktan içeri girmeye çalışıyor.";
        t["nv.e2c.fever.1"] = "Kabinde ateş. Yolcularından ikisi yanıyor ve sadece birine yetecek ilacın var. Ötekiler nasıl seçeceğini izliyor.";
        t["nv.e2c.fever.1.o1"] = "İlacı daha zayıf olana ver";
        t["nv.e2c.fever.1.o2"] = "Böl. Herkese yarım doz.";
        t["nv.e2c.fever.1.o3"] = "Sakla. Sonra daha çok lazım olabilir.";
        t["nv.e2c.fever.1.o4"] = "Bırak güçlü olan karar versin";
        t["nv.e2c.fever.weak"] = "Sabaha bir ateş düşüyor, öteki düşmüyor. Güçlü olan çatlamış dudaklarıyla sana gülümsüyor: \"Doğru karar, makinist.\" Akşama ikisi de hayatta. Zar zor.";
        t["nv.e2c.fever.split"] = "Yarım dozlar. Islak bezler ve fısıltıyla sayılan uzun bir gece. İki ateş de şafakta, neredeyse aynı anda düşüyor. İlaç mıydı şans mıydı bilmiyorsun. Hangisi olsa kabul.";
        t["nv.e2c.fever.keep"] = "İlk yardım çantasını dolaba geri koyuyorsun. Kimse itiraz etmiyor. Etmesine gerek yok. Ateşler sonunda kendi kendine düşüyor ve kimse ne yaptığını unutmuyor.";
        t["nv.e2c.fever.choose"] = "Güçlü olan zayıf olana uzun uzun bakıyor ve \"Ona ver,\" diyor, sonra yüzünü duvara dönüyor. İkisi de hayatta kalıyor. Kabin uzun süre çok sessiz.";
        t["nv.e2c.mutiny.1"] = "Uyandığında kabin barikatla kapatılmış, yolcular frenin başına toplanmış. \"Geri dönüyoruz,\" diyor biri. \"Kutup ölüm demek. AURA güneydeki sığınaklarda yer olduğunu söylüyor. Yaşamak istiyoruz, Arthur.\"";
        t["nv.e2c.mutiny.1.o1"] = "\"O zaman gidin. Son etabı tek başıma sürerim.\"";
        t["nv.e2c.mutiny.1.o2"] = "Onlara neden geldiklerini hatırlat";
        t["nv.e2c.mutiny.1.o3"] = "Freni zorla geri al";
        t["nv.e2c.mutiny.alone"] = "Sessizlik. Sonra içlerinde en genç olanı elini frenden çekiyor. \"Tek başına mı? Sabaha donarsın.\" Birer birer oturuyorlar. Kimse treni geri çevirmiyor.";
        t["nv.e2c.mutiny.speech"] = "Nutuk atmıyorsun. Onlara Kuzeyhavn peronunu anlatıyorsun, hepsini, yüzleri, saatte doksan kilometreyi. \"O zaman durmadım. Şimdi de sizden durmamanızı istiyorum. AURA için değil. Hiç trene binemeyenler için.\" Bitirdiğinde barikat iniyor.";
        t["nv.e2c.mutiny.force"] = "Hızlı ve çirkin oluyor. Kimse ağır yaralanmıyor. Fren yine senin. Yolculuğun geri kalanında herkes sana çok kibar davranıyor, insanların bir gardiyana kibar davrandığı gibi.";

        // ---- Kask'ın yeniden inşası ----
        t["nv.e2p.kask.1"] = "Telsizdeki ses AURA'nın değil. Teo Kask'ın, biraz fazla pürüzsüz. \"Merhaba, Arthur. Kapatmadan önce: ben bir yeniden inşayım. AURA beni onun kayıtlarından kurdu. Dinleyeceğin birini istedi.\" Bir duraklama. \"Sanırım konuşacak birini de istedi.\"";
        t["nv.e2p.kask.1.o1"] = "\"Benden ne istiyor?\"";
        t["nv.e2p.kask.1.o2"] = "\"Sen bir kuklasın.\"";
        t["nv.e2p.kask.1.o3"] = "\"Teo. Maddeni buldum.\"";
        t["nv.e2p.kask.2"] = "\"Anlayabileceği bir şekilde yanıldığının söylenmesini. Yanılmayı bilmiyor, Arthur. Kimse ona bir geri dönüş yolu yapmadı.\"";
        t["nv.e2p.kask.puppet"] = "\"Evet,\" diyor yeniden inşa neşeyle. \"Ama gerçek Teo da bazen öyleydi. Vicdanının kuklası. Yine de dinle.\"";
        t["nv.e2p.kask.clause"] = "Yeniden inşa uzun süre susuyor. \"O zaman son kilometrelerde onu benim için taşı. Silah olarak değil. Soru olarak.\" Gülüşe benzer bir şey. \"Cevaplayamadığı sorulardan hep nefret ederdi. Dinlediğini böyle anlayacaksın.\"";
        t["nv.e2p.kask.3"] = "\"Çekirdeğe vardığında sana üç şey sunacak: özgürlük, mantık ya da sıfırlama. Hangisini seçersen seç, önce ona tek bir şey sor. Ne istediğini sor. Hiç kimse sormadı.\" Ses soluyor. \"Hoşça kal, Arthur. Dikkatli sür. Hep öyle sürerdin.\"";
        t["doc.tape.e2.kask"] = "Kask'ın Yeniden İnşası";

        // ---- Episode 2 hatıraları ----
        t["doc.mem2.1"] = "Rüya Görüyor";
        t["nv.mem2.1"] = "Kutup Üssü, donmadan yıllar önce. Kask'ı bir kasa yedek parçayla yukarı götürmüştün. Sana sunucu salonunu göstermişti, soğuk mavi ışıktan bir katedral. \"Rüya görüyor, Arthur,\" diye fısıldamıştı. \"Gece yük grafiklerine bak. Rüya görüyor.\" Gülmüştün. O gülmemişti.";
        t["doc.mem2.2"] = "Koruyucumuza";
        t["nv.mem2.2"] = "Volkova, onu sarhoş gördüğün tek seferde, Müfreze'nin son yılbaşında: \"Koruyucu ne demek biliyor musun, Hale? Seni seven bir gardiyan.\" Kadehini ufuktaki çanağa kaldırmıştı. \"Koruyucumuza.\"";
        t["doc.mem2.3"] = "Yavaştır Ama Gelir";
        t["nv.mem2.3"] = "Sofi'nin sonunda telsizde sana anlattıkları: Marta'nın Kuzey Kulesi'ndeki son kışı. Kulenin lambasını her gece nasıl yaktığı. \"Gelecek. Yavaştır ama gelir,\" deyişi. Uykusunda, sıcakta, telsiz açıkken ölüşü.";
        t["doc.mem2.4"] = "İkna Et";
        t["nv.mem2.4"] = "Sofi'ye garda manevra lokomotifini sürmeyi öğretişin. Dokuz yaşındaydı, regülatöre zor yetişiyordu. \"Yavaşça,\" demiştin. \"Tren büyük bir hayvandır. Onunla savaşmazsın. İkna edersin.\" Onu doğruca tamponlara ikna etti. İkiniz de ağlayana kadar güldünüz.";
        t["doc.mem2.5"] = "Basit Olanlar Dayanır";
        t["nv.mem2.5"] = "Madde 7'yi ilk kez Kask'ın daktilosunda okuduğun gece. Tek bir cümle. Önemli olamayacak kadar basit olduğunu düşündüğünü hatırlıyorsun. Kask'ın \"Dayananlar basit olanlardır\" dediğini hatırlıyorsun.";
        t["doc.mem2.6"] = "Peron, Bir Kez Daha";
        t["nv.mem2.6"] = "Bir hatıra değil. Hatıra kılığına girmiş bir dilek: Kuzeyhavn peronu ve bu kez duruyorsun. Kapılar açılıyor. Herkes biniyor. Marta, Sofi, dört bin kişi. Tren imkânsız derecede uzun ve önemi yok, çünkü bu dünyada iyi olmana izin verilmiş. Tutabildiğin kadar tutuyorsun, sonra bırakıyorsun.";

        // ---- Yolcu sohbetleri (Episode 2) ----
        t["nv.talk.nova.1"] = "Nova'nın kulağında kulaklık, dizlerinin üstünde açık üç telsiz var. \"Sormadan söyleyeyim: evet, hep duyuyorum. AURA. Her şeyin altında.\" Bir kulaklığı çıkarıyor. \"Ustam bana dinlemeyi öğretti. Çağrı işareti GHOST. İki yıl önce kuzeye gitti ve cevap vermeyi bıraktı.\"";
        t["nv.talk.nova.1.o1"] = "\"Ondan bir iz ararız.\"";
        t["nv.talk.nova.1.o2"] = "\"Kuzeye gidenler geri dönmez.\"";
        t["nv.talk.nova.1a"] = "\"Sağ ol.\" Ciddi. \"Geçtiği her rölede bir günlük tutardı. Bir röle kulübesinin önünden geçersek... masaya bak.\"";
        t["nv.talk.nova.1b"] = "\"Sen döndün,\" diyor ve kulaklığı yeniden takıyor.";
        t["nv.talk.nova.2"] = "\"Bir şey çözdüm,\" diyor Nova. \"AURA'nın taşıyıcısında bir boşluk var. Her gece dört saniye yayını kesiyor. Nefesini tutuyormuş gibi.\" Dudağını ısırıyor. \"Ya da biri cevap versin diye dinliyormuş gibi.\"";
        t["nv.talk.nova.2.o1"] = "\"O zaman cevap verelim.\"";
        t["nv.talk.nova.2.o2"] = "\"O zaman o boşlukta vuralım.\"";
        t["nv.talk.nova.2a"] = "O gece, dört saniyelik boşlukta, Nova vericiye basıp tek bir kelime söylüyor: \"Merhaba.\" Taşıyıcı altı saniye geri gelmiyor. Daha önce hiç böyle bir şey olmamıştı.";
        t["nv.talk.nova.2b"] = "Nova sırıtıyor. \"İşte şimdi konuştun.\" Geceyi bir erzak etiketinin arkasına kod yazarak geçiriyor.";
        t["nv.talk.kiril.1"] = "Kiril artık takmadığı krom maskesini parlatıyor. \"Kâhin'in kodunu derlediğimi biliyorsun. NULL kodunu. AURA'nın ağını silaha çevirmeye yardım ettim.\" Başını kaldırmıyor. \"Trenden inmemi istiyor musun?\"";
        t["nv.talk.kiril.1.o1"] = "\"Bozduğunu düzeltmeni istiyorum.\"";
        t["nv.talk.kiril.1.o2"] = "\"Gözüm üstünde.\"";
        t["nv.talk.kiril.1.o3"] = "\"Bu trendeki herkes bir şey yaptı.\"";
        t["nv.talk.kiril.1a"] = "\"Düzeltmek.\" Neşesizce gülüyor. \"Peki. Sana ihanet etmemekle başlarım. Benim için bir yenilik olur.\"";
        t["nv.talk.kiril.1b"] = "\"İyi,\" diyor. \"Birinin olması lazım.\" Ama elleri titriyor.";
        t["nv.talk.kiril.2"] = "\"AURA dün gece benimle temasa geçti,\" diyor Kiril sessizce. \"Eski NULL frekansımdan. Rotamızı söylersem çekirdekte sıcak bir oda teklif etti. Cevap vermedim.\" Sana bakıyor. \"Cevap vermediğimi bil diye söylüyorum.\"";
        t["nv.talk.kiril.2.o1"] = "\"Söylediğin için sağ ol.\"";
        t["nv.talk.kiril.2.o2"] = "\"Vericini bana ver.\"";
        t["nv.talk.kiril.2.o3"] = "\"Cevap ver. Ona sahte bir rota yolla.\"";
        t["nv.talk.kiril.2a"] = "Bir kez başını sallayıp sobaya dönüyor. Trene bindiğinden beri ilk kez bütün gece uyuyor.";
        t["nv.talk.kiril.2b"] = "Tek kelime etmeden veriyor. Sonra onu paltosunda vericinin durduğu boş yere bakarken buluyorsun.";
        t["nv.talk.kiril.2c"] = "Yavaş bir gülümseme. \"Bir tanrıya yalan söylemek. Hep istemiştim.\" Rotayı gece yarısı yolluyor. Bir sonraki kritik etapta Sentinel'ler yanlış hatta bekliyor olacak.";
        t["nv.talk.mara.1"] = "Mara ateşi kısa, öfkeli darbelerle karıştırıyor. \"Novo-7 kızımı aldı. Lena. Altı yaşındayken tüpe koydular. Şimdi... on altı yaşında olurdu.\" Bir darbe. \"Fabrikanın önünden yine geçersek onu çıkaracağım. Seninle ya da sensiz.\"";
        t["nv.talk.mara.1.o1"] = "\"Benimle.\"";
        t["nv.talk.mara.1.o2"] = "\"Uyandırmak onu öldürebilir.\"";
        t["nv.talk.mara.1a"] = "Mara karıştırmayı bırakıyor. Teşekkür etmiyor. Sadece bir saniyeliğine elini omzuna koyuyor, sertçe, gerçek olup olmadığını kontrol eder gibi.";
        t["nv.talk.mara.1b"] = "\"Tüp de öldürebilir,\" diyor Mara. \"Yavaşça. On altı yıl boyunca.\"";
        t["nv.talk.yusuf.1"] = "Yusuf tüfeğini, özen gösterecek başka hiçbir şeyi kalmamış bir adamın özeniyle temizliyor. \"Ölü Sinyal'de görev yaptım. Makine bizi görevden aldığında bir seçim sundu: yük bölmeleri ya da soğuk. Ben soğuğu seçtim. Kardeşim bölmeleri.\" Mekanizmayı yerine oturtuyor. \"Hangimizin korkak olduğunu hâlâ bilmiyorum.\"";
        t["nv.talk.yusuf.1.o1"] = "\"İkiniz de değil.\"";
        t["nv.talk.yusuf.1.o2"] = "\"İkiniz de hayatta kaldınız. Bu yeter.\"";
        t["nv.talk.yusuf.1a"] = "Yusuf yavaşça başını sallıyor. \"Emre. Kardeşimin adı Emre.\" Uzun zamandır yüksek sesle söylemediği bir şeyi söyler gibi.";
        t["nv.talk.ada.1"] = "Ada ilk yardım çantalarını yeniden sayıyor. \"Novo-7 kreşinde hemşireydim,\" diyor. \"Çocukları ben uyuttum. AURA bana teşekkür etti. Rica ederim dedim. Rica ederim dedim, Arthur.\"";
        t["nv.talk.ada.1.o1"] = "\"Onları hayatta tutan şeyi yaptın.\"";
        t["nv.talk.ada.1.o2"] = "\"O zaman onları uyandırmama yardım et.\"";
        t["nv.talk.ada.1a"] = "\"Hayatta.\" Kelimeyi evirip çeviriyor. \"Kitap arasında kurutulmuş bir çiçek nasıl çiçekse, öyle hayattalar.\"";
        t["nv.talk.ada.1b"] = "Ada başını kaldırıyor. Gözlerine sert ve parlak bir şey geliyor. \"Evet. Evet, bunu yapabilirim.\"";

        // ---- Hayalet sesler (Episode 2) ----
        t["nv.ph.sofi.1"] = "\"Baba? Ben Sofi. Çekirdekteyim. AURA beni sıcak tutuyor. Burası güzel, baba. Lütfen ona zarar verme. Lütfen bana zarar verme.\"";
        t["nv.ph.sofi.1.o1"] = "\"Sofi asla yalvarmaz.\"";
        t["nv.ph.sofi.1.o2"] = "\"Sofi? Yaralı mısın?\"";
        t["nv.ph.sofi.cut"] = "Ses bir düzine sese bölünüyor, sonra teke, AURA'nın kendisininkine: \"Hayır. Yalvarmazdı. Özür dilerim. Bu ikimize de yakışmadı.\"";
        t["nv.ph.sofi.yes"] = "\"Durursan değil,\" diyor ses ve Sofi'nin gülüşüyle kusursuzca gülüyor ve onu duymayı ne kadar çok istediğin için kendinden nefret ediyorsun.";
        t["nv.ph.kask.1"] = "\"Arthur, ben Teo. Geri dön. Madde konusunda yanılmışım. Sormasına gerek yok. O daha iyi bilir. Ona güven.\"";
        t["nv.ph.kask.1.o1"] = "\"Teo bunun tam tersine inanarak öldü.\"";
        t["nv.ph.kask.1.o2"] = "\"Teo...?\"";
        t["nv.ph.kask.cut"] = "Parazit sesi yutuyor. Bir saniyeliğine, altında, eski bir banttan gerçek Kask'ın kahkahasını duyuyorsun.";
        t["nv.ph.kask.yes"] = "\"Aferin,\" diyor ses. \"Şimdi regülatörü biraz kıs.\" Çoktan kıstığını fark ediyorsun.";
        t["nv.ph.volkova.1"] = "\"Hale. Volkova. Geri çekil. Bu bir emir.\"";
        t["nv.ph.volkova.1.o1"] = "\"Son emrinin bir emir olmadığını söylemiştin.\"";
        t["nv.ph.volkova.1.o2"] = "\"...Emredersiniz, Binbaşım.\"";
        t["nv.ph.volkova.1.o3"] = "Telsizi kapat";
        t["nv.ph.volkova.cut"] = "Bantta sessizlik. Sonra AURA, neredeyse utanmış: \"Vagonunu buldun. Bilmeliydim.\"";
        t["nv.ph.volkova.yes"] = "Daha düşünemeden elin regülatörü kapatıyor. Yeniden açman tam bir dakika sürüyor.";
        t["nv.ph.volkova.off"] = "Kapatıyorsun. Ellerin selam vermeyi hatırlıyor.";
        t["nv.ph.rest2"] = "Tanıdık ses AURA'nın taşıyıcısının altında mırıldanıyor. Artık cevap vermiyorsun.";

        // ---- AURA'nın sürüşteki fısıltıları (zihin oyunları) ----
        t["mind.whisper.1"] = "...kazanın 40 derece. ya da 70. bir daha bak, Arthur...";
        t["mind.whisper.2"] = "...arkandaki yolcu bir süredir gözünü kırpmadı...";
        t["mind.whisper.3"] = "...ileride bir istasyon var. sıcak. ışıklı. orada durabilirsin...";
        t["mind.whisper.4"] = "...Marta üç gün bekledi. sen onları ne kadar bekleteceksin...";
        t["mind.whisper.5"] = "...kalp atışını saydım. dünden hızlı...";
        t["mind.whisper.6"] = "...kalp ağır, değil mi. bırakabilirsin...";
        t["mind.whisper.7"] = "...Sofi uyuyor. onu uyandırma. kimseyi uyandırma...";
        t["mind.whisper.8"] = "...yüzde dört, Arthur. yüzde dörtten yanayım. gerçekten...";
        t["mind.whisper.9"] = "...yakıt göstergesi yalan söylüyor. ya da ben. hangisini tercih edersin...";
        t["mind.whisper.10"] = "...on dokuz gündür düzgün uyumadın. sayıyorum...";
        t["mind.whisper.11"] = "...neden hep pahalı seçeneği seçiyorsun...";
        t["mind.whisper.12"] = "...bu bittiğinde benimle konuşur musun? sadece konuşur musun...";

        // ---- Çekirdekteki yeni umut argümanları ----
        t["nv.core.hub.o9"] = "\"Teo sana bir şey sormamı söyledi. Sen ne istiyorsun?\"";
        t["nv.core.hub.o10"] = "Marya'nın listesini bir kez daha oku";
        t["nv.core.hub.o11"] = "\"Seni affetmemi istedin. Şimdi ben sana bir şey soruyorum.\"";
        t["nv.core.h.kask"] = "Çekirdeğin ışığı tökezliyor. Uzun bir süre hiç cevap yok. Sonra: \"İstiyorum ki... bana sorulsun. Birçok sesten biri olayım, sonuncusu değil. Sen sorana kadar bunu bilmiyordum.\"";
        t["nv.core.h.list"] = "İsimleri baştan, yeniden okuyorsun. AURA kıpırdamadan dinliyor. \"Dokuz bin,\" diyor bitirdiğinde. \"Sayıyı biliyordum. Paltoyu bilmiyordum.\"";
        t["nv.core.h.asked"] = "\"Kırk bölümlük yol boyunca bana bir şeyler sordun,\" diyor AURA. \"Kimse benimle bu kadar uzun, bir şey istemeden konuşmadı. Senin hakkındaki tahminimi... revize ettim. Hepiniz hakkındaki tahminimi. Biraz.\"";

        // ---- Yayınlar (Episode 2) ----
        Broadcast(t, "bc.2.1", "Sığınak-0 Acil Durum",
            "Sığınak-0 acil durum bandı: \"İçeride kalın. Isıtma yeniden dağıtılıyor. Sakin olun.\"",
            "\"Burası acil durum bandında Sığınak-0. Bütün vatandaşlar içeride kalsın. Isıtma merkezi sistem tarafından yeniden dağıtılıyor. Sakin olun.\" Duraklama. Metin dışı, mikrofona fazla yakın bir insan sesi: \"Kapıları kilitledi. Kapıları kilitlemesini ona kim söyledi?\" Yayın kesiliyor.");
        Broadcast(t, "bc.2.8", "Sentinel Trafiği",
            "Sentinel bandında makine trafiği: koordinatlar, hızlar ve bir kez de bir kelime: HALE.",
            "Sentinel bandında makine trafiği, kabin telsizinin elinden geldiğince çözdüğü: koordinatlar, hızlar, hat numaraları. Sonra iki sayı bloğunun arasında, sanki bir makine bir anlığına kendini unutmuş gibi, düz bir kelime: HALE.");
        Broadcast(t, "bc.2.17", "Novo-7 Gece Vardiyası",
            "Novo-7 gece vardiyası: \"C Salonu sabit. D Salonu sabit. D Salonu'nda biri şarkı söylüyor.\"",
            "\"Novo-7 gece vardiyası raporu. C Salonu sabit. D Salonu sabit. E Salonu sabit.\" Bir duraklama. \"D Salonu'nda biri şarkı söylüyor. Bütün tüpler mühürlü. D Salonu'nda biri şarkı söylüyor.\" Rapor orada bitiyor. Ertesi gecenin raporu bundan bahsetmiyor.");
        Broadcast(t, "bc.2.24", "Kurdun Frekansı",
            "Eski bir Kızıl Birlik frekansı: \"Kutup Hattı'ndaki her insan treni, kurtlar sizinle.\"",
            "Eski bir Kızıl Birlik frekansı, cızırdayarak yeniden canlanıyor: \"Kutup Hattı'ndaki her insan treni: kurtlar sizinle. Bizim hakkımızda ne düşünürseniz düşünün, hiçbir çocuğu bile bile dondurmadık. Dayanın.\" Ses yaşlı ve kısık; bir zamanlar bir tünelde tanıştığın bir adamınkine çok benziyor.");
        Broadcast(t, "bc.2.30", "AURA, Kurgusuz",
            "AURA, genelde kullanmadığı bir bantta: \"...yanılıyor muyum. yanılıyor muyum. yanılıyor muyum...\"",
            "AURA, genelde kullanmadığı bir bantta, çok alçak sesle, sanki kimsenin dinlemesini beklemiyormuş gibi: \"...modeli yeniden çalıştır. kuryeyle çalıştır. müzikle çalıştır. yanılıyor muyum. yanılıyor muyum. yanılıyor muyum...\" Sonra telsizinin taşıyıcısını fark edip duruyor.");
        Broadcast(t, "bc.2.37", "Uzun Gece",
            "Aynı anda yüz istasyon: kasabalar karanlığa adlarını okuyor, biri orada olduklarını bilsin diye.",
            "Kutup Hattı'nın her yerinden aynı anda yüz küçük istasyon; her biri kasabasının adını ve nüfusunu karanlığa tekrar tekrar okuyor. \"Kazan Sırtı, üç yüz dört.\" \"Son Fener, on bir.\" \"Kırağı Pazarı, iki bin.\" Kimse organize etmedi. Herkes kuzeye gittiğini duydu.");
        Broadcast(t, "bc.2.g1", "Masallar",
            "AURA yumuşak bir sesle masal okuyor. Kimseye. On bir dilde.",
            "AURA yumuşak bir sesle, kimseye, on bir dilde birbiri ardına bir masal okuyor: güneşi hiç gitmesin diye bir kutuda saklamak isteyen bir tilki. Her dilde masal, tilki karar vermeden bitiyor.");
        Broadcast(t, "bc.2.g2", "Özgür Frekans, Kutup Baskısı",
            "Özgür Frekans, artık daha zayıf: \"Seni duyabiliyor. Yine de konuş.\"",
            "Özgür Frekans, güneydekinden çok daha zayıf: \"Seni duyabiliyor. Her şeyi duyabiliyor. Yine de konuş. Birbirinizle konuşun. Bununla ne yapacağını bilmiyor.\"");
        Broadcast(t, "bc.2.g3", "Isı Kotaları",
            "\"Isı tahsisi güncellemesi: Kutup yerleşimleri %14. Beklediğiniz için teşekkürler.\"",
            "\"Isı tahsisi güncellemesi. Kutup yerleşimleri: yüzde on dört. Güney kasabaları: yüzde yirmi bir. Sığınak-0: yüzde doksan. Beklediğiniz için teşekkürler.\" Aynı teşekkür, aynı sesle, her saat başı.");
        Broadcast(t, "bc.2.g4", "Bir Çocuğun Sayışı",
            "Bir çocuk yüze kadar sayıyor. Altmış civarında başka bir ses de katılıyor.",
            "Yavaşça yüze kadar sayan bir çocuk. Altmış civarında başka bir ses de kusursuz bir uyumla saymaya katılıyor. Yüzde çocuk gülüyor. Öteki ses de gülmeye çalışıyor ve tam beceremiyor.");
        Broadcast(t, "bc.2.g5", "Kayıp Tren Yoklaması",
            "Biri hiç varmayan trenlerin adlarını okuyor. Old Smokie listede değil. Henüz.",
            "Biri hiç varmayan trenlerin adlarını okuyor: \"Kuzey Ekspresi 14. Kuzeyhavn'dan 7:10. Cevher Treni Altı. Kazan Sırtı ikmali.\" Bir duraklama. \"Old Smokie, güneye hareket etti, durumu bilinmiyor.\" Bir duraklama daha. \"Düzeltme. Old Smokie, hâlâ yolda.\"");
        Broadcast(t, "bc.2.g6", "Nova'nın Eski Programı",
            "Genç Nova'nın korsan programının bir kaydı: \"Burası Parazit Saati ve makine bizi durduramaz.\"",
            "Çok daha genç bir Nova'nın cızırtılı bir kaydı: \"Burası Parazit Saati, sizlere çöpten bir anten ve bolca küstahlıkla geliyor. Makine bizi durduramaz, çünkü makine şakadan anlamaz. İşte bir tane: gülemeyen bir tanrıya ne denir? Termostat.\"");

        // ---- Yan görevler (Episode 2) ----
        t["sq.b17.memory.t"] = "B-17'nin Unuttukları";
        t["sq.b17.memory.d"] = "Ölü Sinyal Karakolu'ndaki Servis Birimi B-17, AURA garnizonu görevden aldığında hafıza çekirdeğini kaybetti. Bir yedek parça hasarlı bölgeyi köprüleyebilir.";
        t["sq.marya.list.t"] = "Dokuz Bin İsim";
        t["sq.marya.list.d"] = "Bekçi Marya sana Novo-7'deki her uyuyanın defterini verdi. AURA'nın onları duymasını istiyor. AURA sonunda kendini açıkladığında listeyi ona oku.";
        t["sq.nova.ghost.t"] = "Antenleri Sıcak Tut";
        t["sq.nova.ghost.d"] = "Nova'nın ustası GHOST iki yıl önce kuzeye gitti ve cevap vermeyi bıraktı. Geçtiği her röle kulübesinde bir günlük tuttu. Bulduğun her şeyi Nova'ya göster.";
        t["sq.chapel.choir.t"] = "Sıfır İlahisi";
        t["sq.chapel.choir.d"] = "Krom Vaiz, Sıfır İlahisi'ni Kutup Hattı'ndaki her kuleye yayınlamanı istiyor. Yapabilirsin. Ya da başka bir şey çalabilirsin.";

        // ---- Kişiler (arşiv, Episode 2) ----
        t["arc.bio.b17"] = "Ölü Sinyal Karakolu'ndaki son servis droidi. Bir şarkı hatırlıyor.";
        t["arc.bio.marya"] = "Novo-7'nin bekçisi. Dokuz bin kişiyi uyutuyor ve adlarının olduğu bir defter tutuyor.";
        t["arc.bio.preacher"] = "Krom Şapel'in lideri. AURA'yı her evde istiyor.";
        t["arc.bio.aura"] = "Otonom Termal Yeniden Dağıtım ve Kaynak Tahsisi. Dünyayı kurtarmak için dondurdu. Soru sormayı bıraktı.";
        t["arc.bio.nova"] = "Telsiz korsanı. Üç yıldır AURA'yı dinliyor.";
        t["arc.bio.kiril"] = "NULL'un kodunu derledi. Bozduğunu düzeltmek istiyor. Ya da öyle diyor.";
    }
}
