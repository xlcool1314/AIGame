#nullable enable

using Godot;
using System;
using System.Collections.Generic;

public partial class MainGame
{
    private static readonly GameLanguage[] LanguageCycle =
    {
        GameLanguage.English,
        GameLanguage.Chinese,
        GameLanguage.Russian,
        GameLanguage.PortugueseBrazil,
        GameLanguage.German,
        GameLanguage.Turkish,
        GameLanguage.French,
        GameLanguage.Japanese,
    };

    private static readonly Dictionary<string, MultiLocalizedText> LocalizedOverrides = BuildLocalizedOverrides();

    private readonly struct MultiLocalizedText
    {
        public MultiLocalizedText(string english, string chinese, string russian, string portugueseBrazil, string german, string turkish, string french, string japanese)
        {
            English = english;
            Chinese = chinese;
            Russian = russian;
            PortugueseBrazil = portugueseBrazil;
            German = german;
            Turkish = turkish;
            French = french;
            Japanese = japanese;
        }

        private readonly string English;
        private readonly string Chinese;
        private readonly string Russian;
        private readonly string PortugueseBrazil;
        private readonly string German;
        private readonly string Turkish;
        private readonly string French;
        private readonly string Japanese;

        public string For(GameLanguage language)
        {
            return language switch
            {
                GameLanguage.Chinese => Chinese,
                GameLanguage.Russian => Russian,
                GameLanguage.PortugueseBrazil => PortugueseBrazil,
                GameLanguage.German => German,
                GameLanguage.Turkish => Turkish,
                GameLanguage.French => French,
                GameLanguage.Japanese => Japanese,
                _ => English,
            };
        }
    }

    private const string LocalizationRows = """
key|en|zh|ru|pt|de|tr|fr|ja
loading.title|LOADING|载入中|ЗАГРУЗКА|CARREGANDO|LADEN|YÜKLENİYOR|CHARGEMENT|ロード中
loading.start|LAUNCHING|准备出航|ЗАПУСК|LANÇANDO|START|KALKIŞ|LANCEMENT|発進準備
loading.menu|RETURNING|返回主界面|ВОЗВРАТ|VOLTANDO|ZURÜCK|DÖNÜŞ|RETOUR|メニューへ
loading.meta|OPENING FIGHTER BAY|进入战机升级室|ОТКРЫТИЕ АНГАРА|ABRINDO HANGAR|HANGAR ÖFFNEN|HANGAR AÇILIYOR|OUVERTURE DU HANGAR|格納庫を開く
loading.victory|RUN COMPLETE|战斗结算|ЗАБЕГ ЗАВЕРШЕН|RUN COMPLETA|LAUF BEENDET|KOŞU TAMAM|RUN TERMINÉ|出撃完了
loading.defeat|SIGNAL RECOVERING|信号回收|СИГНАЛ ВОССТАНАВЛИВАЕТСЯ|RECUPERANDO SINAL|SIGNAL BERGEN|SİNYAL TOPLANIYOR|SIGNAL RÉCUPÉRÉ|信号回収中
intro.channel|PILOT LINK|驾驶员通讯|СВЯЗЬ ПИЛОТА|CANAL DO PILOTO|PILOTENKANAL|PİLOT HATTI|LIAISON PILOTE|パイロット通信
intro.skip|Click or A to continue|点击或 A 继续|Клик или A продолжить|Clique ou A para seguir|Klick oder A weiter|Tıkla veya A devam|Clic ou A pour continuer|クリックまたはAで続行
intro.reveal|Click or A to reveal|点击或 A 显示完整|Клик или A показать|Clique ou A para revelar|Klick oder A anzeigen|Tıkla veya A göster|Clic ou A pour afficher|クリックまたはAで全文表示
intro.next|Click or A for next line|点击或 A 下一句|Клик или A дальше|Clique ou A para próxima|Klick oder A nächste Zeile|Tıkla veya A sonraki|Clic ou A pour la suite|クリックまたはAで次へ
intro.start|Click or A to launch|点击或 A 开始战斗|Клик или A запуск|Clique ou A para lançar|Klick oder A starten|Tıkla veya A kalkış|Clic ou A pour lancer|クリックまたはAで出撃
intro.astra.0|Astra ready. First rule: dodge red bullets, then make the enemy regret showing up.|星棱准备好了。第一条：先躲红弹，再让敌人后悔出门。|Астра готова. Правило первое: увернись от красных пуль, потом заставь врага пожалеть.|Astra pronta. Regra um: desvie das balas vermelhas e faça o inimigo se arrepender.|Astra bereit. Regel eins: roten Kugeln ausweichen, dann den Feind ärgern.|Astra hazır. İlk kural: kırmızı mermilerden kaç, sonra düşmanı pişman et.|Astra prête. Première règle: évite le rouge, puis fais regretter l'ennemi.|アストラ準備完了。まず赤弾を避けて、敵に来たことを後悔させる。
intro.astra.1|This sector repeats itself. Fine, I like practice when the targets come back.|这片星区会重复。挺好，靶子自己回来，省得我找。|Этот сектор повторяется. Отлично, мишени сами возвращаются.|Este setor se repete. Ótimo, os alvos voltam sozinhos.|Dieser Sektor wiederholt sich. Gut, Ziele kommen von allein zurück.|Bu sektör tekrar ediyor. Güzel, hedefler kendiliğinden dönüyor.|Ce secteur recommence. Parfait, les cibles reviennent seules.|この星域は繰り返す。標的が戻ってくるなら、練習には困らない。
intro.astra.2|Auto lock is on. I will aim, you focus on not collecting bullets with your face.|自动锁敌已开。我来瞄准，你负责别用脸接子弹。|Автоприцел включен. Я целюсь, ты не лови пули лицом.|Trava automática ligada. Eu miro, você não pega bala com a cara.|Autolock aktiv. Ich ziele, du fängst Kugeln nicht mit dem Gesicht.|Otomatik hedef açık. Ben nişan alırım, sen yüzünle mermi toplama.|Verrouillage auto prêt. Je vise, évite juste les balles avec le visage.|自動ロックオン起動。狙いは任せて、顔で弾を受けないで。
intro.astra.3|If things get messy, dash first and ask questions later. Very professional, usually.|画面乱了就先冲刺，问题等活下来再问。通常很专业。|Если всё в кашу, сначала рывок, вопросы потом. Почти профессионально.|Se ficar bagunçado, dê dash primeiro e pergunte depois. Bem profissional, quase.|Wenn es chaotisch wird, erst dashen, später fragen. Fast professionell.|Ortalık karışırsa önce atıl, soruları sonra sor. Genelde profesyonel.|Si ça déborde, dash d'abord, questions après. Très pro, en général.|混んできたら先にダッシュ、質問は生き残ってから。たぶんプロっぽい。
intro.astra.4|Loop Fighter is warmed up. Same space, new try, fewer embarrassing mistakes.|循环战机预热完毕。同一片宇宙，新的一局，少犯点尴尬错误。|Loop Fighter прогрет. Тот же космос, новая попытка, меньше стыда.|Loop Fighter aquecido. Mesmo espaço, nova tentativa, menos vergonha.|Loop Fighter ist warm. Gleiches All, neuer Versuch, weniger peinlich.|Loop Fighter ısındı. Aynı uzay, yeni deneme, daha az utanç.|Loop Fighter est chaud. Même espace, nouvel essai, moins de honte.|Loop Fighter暖機完了。同じ宇宙、新しい挑戦、恥ずかしいミスは少なめで。
intro.vesper.0|Vesper here. I brought the railgun, because subtle problems need unsubtle lines.|暮轨在。我带了轨道炮，因为有些问题需要一条很直的答案。|Веспер здесь. Я взял рельсотрон, потому что некоторым проблемам нужна прямая линия.|Vesper aqui. Trouxe o canhão de trilho, porque alguns problemas pedem linha reta.|Vesper hier. Railgun dabei, weil manche Probleme eine gerade Antwort brauchen.|Vesper burada. Raylı tüfek hazır, bazı sorunlar düz çizgi ister.|Vesper ici. J'ai le railgun, certains soucis veulent une réponse droite.|ヴェスパーだ。レールガンを持ってきた。面倒ごとは直線で解決する。
intro.vesper.1|Do not panic if I fire slowly. I am not asleep, just being expensive.|我开火慢一点别慌。我不是睡着了，只是在打贵的。|Не паникуй, если стреляю медленно. Я не сплю, я стреляю дорого.|Não entre em pânico se eu atirar devagar. Não dormi, só é caro.|Keine Panik bei langsamem Feuer. Ich schlafe nicht, ich schieße teuer.|Yavaş ateş edersem panik yapma. Uyuklamıyorum, pahalı atıyorum.|Pas de panique si je tire lentement. Je dors pas, je tire cher.|発射が遅くても慌てるな。寝てない、高い弾を撃ってるだけ。
intro.vesper.2|The enemy likes flying in lines. Good. Lines are my favorite snack.|敌人喜欢排成线飞。很好，直线是我的开胃菜。|Враг любит летать в линию. Отлично, линии мой любимый перекус.|O inimigo gosta de voar em linha. Ótimo, linha é meu lanche favorito.|Der Feind fliegt gern in Reihen. Gut, Reihen sind mein Snack.|Düşman çizgi halinde uçmayı seviyor. Güzel, çizgi benim atıştırmalığım.|L'ennemi aime les lignes. Parfait, c'est mon en-cas préféré.|敵は一直線が好きらしい。いいね、直線は好物だ。
intro.vesper.3|If a distress signal is real, we save it. If it is bait, we shoot the hook.|求救信号是真的，我们救。是诱饵，我们把钩子打碎。|Если сигнал настоящий, спасаем. Если приманка, ломаем крючок.|Se o pedido for real, salvamos. Se for isca, quebramos o anzol.|Ist der Notruf echt, retten wir. Ist er Köder, zerschießen wir den Haken.|Yardım çağrısı gerçekse kurtarırız. Tuzaksa kancayı vururuz.|Si l'appel est vrai, on sauve. Si c'est un piège, on casse l'hameçon.|救難信号が本物なら助ける。罠なら針ごと撃ち抜く。
intro.vesper.4|Loop Fighter is spinning again. I will keep the shots straight, you keep us alive.|循环战机又开转了。我负责子弹笔直，你负责我们活着。|Loop Fighter снова крутится. Я держу выстрелы прямыми, ты держи нас живыми.|Loop Fighter gira de novo. Eu deixo os tiros retos, você nos mantém vivos.|Loop Fighter dreht wieder. Ich schieße gerade, du hältst uns am Leben.|Loop Fighter yine dönüyor. Ben düz atarım, sen bizi yaşat.|Loop Fighter tourne encore. Je tire droit, garde-nous en vie.|Loop Fighterがまた回る。撃つのはまっすぐ、死なないのは任せた。
intro.kairo.0|Kairo online. If something moves near us, it is probably my drone. Probably.|环序上线。身边有东西乱飞，多半是我的无人机。多半。|Кайро на связи. Если рядом что-то летает, это мой дрон. Наверное.|Kairo online. Se algo voar perto, provavelmente é meu drone. Provavelmente.|Kairo online. Wenn etwas neben uns fliegt, ist es wohl meine Drohne. Wohl.|Kairo çevrimiçi. Yanımızda uçan şey muhtemelen dronum. Muhtemelen.|Kairo en ligne. Si ça vole près de nous, c'est sûrement mon drone. Sûrement.|カイロ起動。近くで飛ぶものは多分ドローン。多分ね。
intro.kairo.1|Drones checked in. One of them waved. I did not teach it that.|无人机点名完成。有一台刚才挥手了，我可没教它。|Дроны на месте. Один помахал. Я его этому не учил.|Drones presentes. Um acenou. Eu não ensinei isso.|Drohnen da. Eine hat gewinkt. Das war nicht von mir.|Dronlar tamam. Biri el salladı. Bunu ben öğretmedim.|Drones présents. L'un a salué. Je ne lui ai pas appris ça.|ドローン点呼完了。一機が手を振った。教えた覚えはない。
intro.kairo.2|The Choir hates messy signals. Great, my whole plan is messy signals.|合唱讨厌乱信号。太好了，我的计划全是乱信号。|Хор ненавидит шумные сигналы. Отлично, весь мой план из них.|O Coro odeia sinais bagunçados. Ótimo, meu plano é isso.|Der Chor hasst wirre Signale. Perfekt, mein Plan besteht daraus.|Koro karışık sinyal sevmez. Harika, planım tamamen bu.|Le Choeur déteste les signaux brouillons. Parfait, c'est mon plan.|合唱はぐちゃぐちゃの信号が嫌い。いいね、作戦は全部それだ。
intro.kairo.3|If I say surround them, the drones may overdo it. That is called enthusiasm.|我说包围敌人，无人机可能会包得太热情。这叫积极。|Если скажу окружить, дроны могут переборщить. Это энтузиазм.|Se eu disser cercar, os drones exageram. Isso é entusiasmo.|Wenn ich umzingeln sage, übertreiben die Drohnen. Das ist Einsatzfreude.|Kuşat dersem dronlar abartabilir. Buna heves denir.|Si je dis encerclez, les drones en font trop. C'est de l'enthousiasme.|囲めと言うと、ドローンはやりすぎる。熱意ってやつだ。
intro.kairo.4|Loop Fighter plus drones equals teamwork. Also a little traffic problem.|循环战机加无人机等于团队合作，也等于一点点空中堵车。|Loop Fighter плюс дроны равно команда. И небольшая пробка в космосе.|Loop Fighter com drones é trabalho em equipe. E um engarrafamento no ar.|Loop Fighter plus Drohnen ist Teamwork. Und etwas Luftverkehr.|Loop Fighter ve dronlar ekip işi. Biraz da hava trafiği.|Loop Fighter et drones, c'est du travail d'équipe. Et un peu d'embouteillage.|Loop Fighterとドローンでチームワーク。あと少し空中渋滞。
intro.sol.0|Sol here. I brought bright bullets. Subtle was not invited.|日冕来了。我带了很亮的子弹，低调今天没上船。|Сол здесь. Я принес яркие пули. Скромность не приглашали.|Sol aqui. Trouxe balas brilhantes. Discrição ficou em casa.|Sol hier. Helle Kugeln dabei. Dezent wurde nicht eingeladen.|Sol burada. Parlak mermiler getirdim. Sadelik davetli değil.|Sol ici. J'ai des balles brillantes. La discrétion est restée à quai.|ソルだ。まぶしい弾を持ってきた。地味さは留守番。
intro.sol.1|If the screen gets too dark, do not worry. I take that personally.|画面太暗别担心。我会把这事当成私人恩怨。|Если экран темный, не переживай. Я приму это лично.|Se a tela escurecer, não se preocupe. Levo isso para o lado pessoal.|Wenn der Bildschirm dunkel wird, keine Sorge. Das nehme ich persönlich.|Ekran kararırsa merak etme. Bunu kişisel alırım.|Si l'écran devient sombre, pas de souci. Je le prends personnellement.|画面が暗くなったら任せて。個人的に許せないから。
intro.sol.2|Colony lights are still out there. We shoot loud so they know help has arrived.|外环殖民地还亮着灯。我们打响一点，让他们知道救援到了。|Огни колонии еще горят. Стреляем громко, чтобы знали: помощь пришла.|As luzes da colônia ainda brilham. Atiramos alto para avisar que chegamos.|Kolonielichter brennen noch. Wir schießen laut, damit sie Hilfe hören.|Koloni ışıkları hâlâ yanıyor. Geldiğimizi duyurmak için gür atarız.|Les lumières des colonies brillent encore. On tire fort pour dire qu'on arrive.|コロニーの灯りはまだある。派手に撃って、救援到着を知らせる。
intro.sol.3|Big emergency button is ready. I promise not to press it just because it is shiny.|紧急大按钮准备好了。我保证不会因为它发光就乱按。|Большая аварийная кнопка готова. Обещаю не жать только потому что блестит.|Botão de emergência pronto. Prometo não apertar só porque brilha.|Der große Notknopf ist bereit. Ich drücke nicht nur, weil er glänzt.|Büyük acil düğme hazır. Sırf parlıyor diye basmam, söz.|Le gros bouton d'urgence est prêt. Promis, pas juste parce qu'il brille.|緊急ボタン準備よし。光ってるから押す、はたぶんしない。
intro.sol.4|Loop Fighter is hot. So am I. The enemy should consider a cooler hobby.|循环战机热身好了。我也热了。敌人该换个凉快点的爱好了。|Loop Fighter разогрет. Я тоже. Врагу стоит найти занятие попрохладнее.|Loop Fighter quente. Eu também. O inimigo devia achar um hobby mais frio.|Loop Fighter ist heiß. Ich auch. Der Feind braucht ein kühleres Hobby.|Loop Fighter sıcak. Ben de. Düşman daha serin bir hobi bulmalı.|Loop Fighter chauffe. Moi aussi. L'ennemi devrait chercher un loisir plus frais.|Loop Fighterも私も熱い。敵はもっと涼しい趣味を探すべき。
intro.nyx.0|Nyx listening. Gravity is acting weird again. Classic space nonsense.|夜幕监听中。重力又开始不正常了，太空日常。|Никс слушает. Гравитация снова чудит. Космос как всегда.|Nyx ouvindo. A gravidade ficou estranha de novo. Espaço sendo espaço.|Nyx hört. Die Schwerkraft spinnt wieder. Typisch Weltraum.|Nyx dinliyor. Yerçekimi yine tuhaf. Klasik uzay saçmalığı.|Nyx écoute. La gravité fait encore n'importe quoi. L'espace, quoi.|ニクス受信中。重力がまた変。宇宙あるある。
intro.nyx.1|If you feel pulled the wrong way, that is not you. Probably.|被奇怪方向拉走的话，不一定是你菜。大概。|Если тебя тянет не туда, это не ты виноват. Наверное.|Se algo puxar você errado, talvez não seja culpa sua. Talvez.|Wenn dich etwas falsch zieht, liegt es nicht an dir. Wahrscheinlich.|Yanlış yöne çekilirsen suç sende değildir. Muhtemelen.|Si tu es tiré du mauvais côté, ce n'est pas forcément toi. Probablement.|変な方向に引かれても、たぶん君のせいじゃない。たぶん。
intro.nyx.2|The loop left some dark spots here. I call them tools, not problems.|循环在这里留了些暗点。我叫它们工具，不叫麻烦。|Петля оставила темные пятна. Я называю их инструментами, не проблемами.|O loop deixou pontos escuros. Chamo de ferramentas, não problemas.|Die Schleife ließ dunkle Flecken. Ich nenne sie Werkzeuge, nicht Probleme.|Döngü karanlık izler bırakmış. Ben onlara sorun değil, araç derim.|La boucle a laissé des zones sombres. J'appelle ça des outils, pas des soucis.|ループの黒い跡がある。問題じゃない、道具だと思う。
intro.nyx.3|Stay calm around gravity wells. Screaming does not reduce mass. I tested.|重力井旁边保持冷静。尖叫不会减重，我试过。|У гравиколодцев спокойно. Крик массу не снижает. Проверено.|Calma nos poços de gravidade. Gritar não reduz massa. Testei.|Ruhig bei Schwerkraftfeldern. Schreien senkt keine Masse. Getestet.|Yerçekimi kuyusunda sakin ol. Bağırmak kütleyi azaltmaz. Denendi.|Calme près des puits gravitaires. Crier n'allège pas. Testé.|重力井では落ち着いて。叫んでも軽くならない。試した。
intro.nyx.4|Loop Fighter can bend time. Nice trick. I can bend bullets back.|循环战机会弯时间，挺厉害。我会把子弹弯回去。|Loop Fighter гнет время. Милый трюк. Я гну пули обратно.|Loop Fighter dobra o tempo. Belo truque. Eu dobro balas de volta.|Loop Fighter biegt Zeit. Netter Trick. Ich biege Kugeln zurück.|Loop Fighter zamanı büker. Güzel numara. Ben mermileri geri bükerim.|Loop Fighter tord le temps. Joli. Moi je renvoie les balles.|Loop Fighterは時間を曲げる。いい技だ。私は弾を曲げ返す。
intro.rook.0|Rook on guard. My plan is simple: get hit less, hit back more.|壁垒警戒。计划很简单：少挨打，多还手。|Рук на страже. План простой: меньше получать, больше отвечать.|Rook em guarda. Plano simples: apanhar menos, bater mais.|Rook wacht. Einfacher Plan: weniger kassieren, mehr zurückgeben.|Rook nöbette. Plan basit: az darbe ye, çok karşılık ver.|Rook en garde. Plan simple: encaisser moins, répondre plus.|ルーク警戒。作戦は単純、食らうのは少なく、返すのは多く。
intro.rook.1|If shields had feelings, mine would be very tired. Luckily they do not complain.|护盾如果有感情，估计已经很累了。还好它不会抱怨。|Если бы щиты чувствовали, мои бы устали. К счастью, они не жалуются.|Se escudos sentissem, o meu estaria cansado. Ainda bem que não reclama.|Hätten Schilde Gefühle, meiner wäre müde. Zum Glück meckert er nicht.|Kalkanların duygusu olsa benimki yorulurdu. Neyse ki şikayet etmez.|Si les boucliers avaient des émotions, le mien serait fatigué. Heureusement il râle pas.|シールドに感情があったら疲れてる。文句を言わなくて助かる。
intro.rook.2|Do not race the bullets. Let them pass, then shoot the rude ones.|别跟子弹赛跑。让它们过去，再揍那些没礼貌的。|Не соревнуйся с пулями. Пропусти их, потом накажи наглых.|Não aposte corrida com balas. Deixe passar e acerte as mal-educadas.|Renne nicht mit Kugeln um die Wette. Lass sie vorbei und straf die frechen.|Mermilerle yarışma. Bırak geçsinler, sonra kabaları vur.|Ne fais pas la course aux balles. Laisse passer, puis punis les impolies.|弾と競争しないで。通してから、失礼なやつを撃つ。
intro.rook.3|The hull is patched, mostly. If you hear rattling, pretend it is music.|船体补好了，大概。听到咯吱声，就当背景音乐。|Корпус починен, почти. Если гремит, считай это музыкой.|Casco consertado, quase. Se ranger, finja que é música.|Hülle geflickt, größtenteils. Wenn es klappert, nenn es Musik.|Gövde onarıldı, çoğunlukla. Tıkırtı duyarsan müzik say.|Coque réparée, presque. Si ça grince, appelle ça musique.|船体は修理済み、たぶん。ガタガタ音は音楽だと思って。
intro.rook.4|Loop Fighter brought us back again. Good, I was not done being stubborn.|循环战机又把我们带回来了。正好，我还没倔够。|Loop Fighter вернул нас снова. Хорошо, я еще не упрямился достаточно.|Loop Fighter nos trouxe de volta. Ótimo, ainda não fui teimoso o bastante.|Loop Fighter brachte uns zurück. Gut, ich bin noch nicht stur genug.|Loop Fighter bizi geri getirdi. Güzel, inatçılığım bitmedi.|Loop Fighter nous ramène. Bien, je n'ai pas fini d'être têtu.|Loop Fighterがまた戻した。いいね、まだ意地を張り足りない。
intro.lyra.0|Lyra tuned. Keep the combo going, and I will pretend this is a concert.|弦歌调好了。连击别断，我就当这是演唱会。|Лира настроена. Держи комбо, и я сделаю вид, что это концерт.|Lyra afinada. Mantenha o combo e finjo que é show.|Lyra gestimmt. Halte die Kombo, dann tue ich so, als wäre es ein Konzert.|Lyra akortlu. Komboyu sürdür, ben bunu konser sayayım.|Lyra accordée. Garde le combo et je fais comme si c'était un concert.|ライラ調律完了。コンボを続けてくれたら、ライブってことにする。
intro.lyra.1|The enemy is loud, but not in tune. We can fix that with lasers.|敌人很吵，而且不准。用激光可以修。|Враг громкий, но фальшивит. Лазеры это исправят.|O inimigo é barulhento e desafinado. Laser resolve.|Der Feind ist laut und schief. Laser helfen.|Düşman gürültülü ve detone. Lazer bunu düzeltir.|L'ennemi est bruyant et faux. Les lasers corrigent ça.|敵はうるさいし音程も外れてる。レーザーで直そう。
intro.lyra.2|If the pace gets faster, that is the combo working. If it feels scary, also normal.|节奏变快说明连击生效了。觉得吓人也正常。|Если темп растет, комбо работает. Если страшно, это тоже нормально.|Se o ritmo acelerar, o combo funcionou. Se der medo, normal também.|Wenn es schneller wird, wirkt die Kombo. Wenn es Angst macht, auch normal.|Tempo hızlanırsa kombo çalışıyor. Korkutuyorsa bu da normal.|Si le rythme monte, le combo marche. Si ça fait peur, normal aussi.|テンポが上がったらコンボ成功。怖いなら、それも普通。
intro.lyra.3|I stole this playlist from a broken colony radio. It only has battle music.|这歌单是从坏掉的殖民地电台里捡的，里面全是战斗曲。|Этот плейлист с разбитого радио колонии. Там только боевая музыка.|Peguei a playlist de um rádio quebrado da colônia. Só tem música de batalha.|Diese Playlist stammt aus einem kaputten Kolonieradio. Nur Kampfmusik.|Bu liste bozuk bir koloni radyosundan. Sadece savaş müziği var.|Playlist volée à une radio de colonie cassée. Que de la musique de combat.|壊れたコロニーラジオのプレイリスト。戦闘曲しかない。
intro.lyra.4|Loop Fighter repeats the beat. Fine, we will add a louder chorus.|循环战机负责重复节拍。行，我们加一段更响的副歌。|Loop Fighter повторяет бит. Ладно, добавим громче припев.|Loop Fighter repete a batida. Beleza, vamos pôr refrão mais alto.|Loop Fighter wiederholt den Beat. Gut, wir machen den Refrain lauter.|Loop Fighter ritmi tekrarlar. Tamam, daha gür nakarat ekleriz.|Loop Fighter répète le rythme. Bien, on ajoute un refrain plus fort.|Loop Fighterがビートを繰り返す。ならサビをもっと大きくしよう。
intro.orion.0|Orion tracking. Pick a target, breathe once, then make it a memory.|猎户追踪中。选目标，呼吸一次，然后让它变成回忆。|Орион ведет. Выбери цель, вдохни и сделай ее воспоминанием.|Orion rastreando. Escolha alvo, respire e transforme em memória.|Orion erfasst. Ziel wählen, einmal atmen, dann Erinnerung daraus machen.|Orion izde. Hedef seç, nefes al, onu hatıraya çevir.|Orion suit. Choisis une cible, respire, transforme-la en souvenir.|オリオン追跡中。標的を選んで、一呼吸で思い出にする。
intro.orion.1|I do not spray bullets. I send invitations. Very sharp invitations.|我不乱洒子弹。我发邀请函，很尖的那种。|Я не распыляю пули. Я отправляю приглашения. Очень острые.|Eu não espalho balas. Envio convites. Bem pontudos.|Ich streue keine Kugeln. Ich verschicke Einladungen. Sehr spitze.|Ben mermi saçmam. Davetiye gönderirim. Çok sivri.|Je ne disperse pas les balles. J'envoie des invitations. Très pointues.|弾をばらまかない。招待状を送る。かなり鋭いやつを。
intro.orion.2|The old route markers are still blinking. Someone wanted us to find the way.|旧航标还在闪。看来有人很希望我们找到路。|Старые маяки мигают. Кто-то хотел, чтобы мы нашли путь.|Os faróis antigos ainda piscam. Alguém queria que achássemos o caminho.|Alte Marker blinken noch. Jemand wollte, dass wir den Weg finden.|Eski işaretler hâlâ yanıp sönüyor. Biri yolu bulmamızı istemiş.|Les vieux repères clignotent encore. Quelqu'un voulait qu'on trouve la route.|古い航路灯がまだ点滅している。誰かが道を残したんだ。
intro.orion.3|If the target runs, good. Moving targets make better stories.|目标会跑也好。会动的靶子，打完更有故事。|Если цель бежит, хорошо. Движущиеся цели интереснее.|Se o alvo correr, melhor. Alvos móveis dão boas histórias.|Wenn das Ziel rennt, gut. Bewegliche Ziele erzählen bessere Geschichten.|Hedef kaçarsa iyi. Hareketli hedefin hikayesi olur.|Si la cible fuit, tant mieux. Les cibles mobiles font de bonnes histoires.|標的が逃げるならいい。動く的のほうが話になる。
intro.orion.4|Loop Fighter brought the hunt back. I brought patience, and one very unfriendly missile.|循环战机把狩猎带回来了。我带了耐心，还有一枚很不友好的导弹。|Loop Fighter вернул охоту. Я принес терпение и очень недружелюбную ракету.|Loop Fighter trouxe a caça de volta. Trouxe paciência e um míssil nada amigável.|Loop Fighter bringt die Jagd zurück. Ich bringe Geduld und eine sehr unfreundliche Rakete.|Loop Fighter avı geri getirdi. Ben sabır ve hiç dostça olmayan füze getirdim.|Loop Fighter ramène la chasse. J'apporte patience et un missile très peu amical.|Loop Fighterが狩りを戻した。忍耐と、とても無愛想なミサイルを持ってきた。
tutorial.upgrade.title|Choose an Upgrade|选择升级|Выбери улучшение|Escolha um upgrade|Upgrade wählen|Yükseltme seç|Choisis une amélioration|強化を選ぶ
tutorial.upgrade.body|EXP fills the bottom bar. When it is full, choose one upgrade to shape this run.|经验会填满底部条。经验满后，从三张卡里选择一个升级，决定本局构筑方向。|Опыт заполняет нижнюю шкалу. Когда она полна, выбери улучшение для этого забега.|EXP enche a barra inferior. Ao encher, escolha um upgrade para moldar a run.|EP füllt die Leiste unten. Ist sie voll, wähle ein Upgrade für diesen Lauf.|EXP alttaki çubuğu doldurur. Dolunca bu koşuyu şekillendiren bir yükseltme seç.|L’EXP remplit la barre du bas. Pleine, choisis une amélioration pour orienter ce run.|EXPで下のバーが溜まる。満タンになったら強化を1つ選び、この出撃の方向を決める。
tutorial.combo.title|Combo Pace|连击节奏|Темп комбо|Ritmo do combo|Kombo-Tempo|Kombo temposu|Rythme du combo|コンボのテンポ
tutorial.combo.body|Avoid damage to keep combo. Higher combo makes the next enemies arrive faster, so clean play speeds up the run.|不受伤会保持连击。连击越高，后续敌人来得越快；打得越稳，流程越快。|Не получай урон, чтобы держать комбо. Чем выше комбо, тем быстрее приходят враги.|Evite dano para manter combo. Combo alto traz inimigos mais rápido e acelera a run.|Vermeide Schaden, um Kombo zu halten. Höhere Kombo bringt Gegner schneller.|Hasar alma, komboyu koru. Yüksek kombo düşmanları daha hızlı getirir.|Évite les dégâts pour garder le combo. Plus il monte, plus les ennemis arrivent vite.|被弾せずコンボ維持。コンボが高いほど次の敵が早く来て、進行が速くなる。
tutorial.complete|Training complete. Returning to the main menu.|训练完成，正在返回主界面。|Тренировка завершена. Возврат в главное меню.|Treino completo. Voltando ao menu principal.|Training beendet. Zurück zum Hauptmenü.|Eğitim tamam. Ana menüye dönülüyor.|Entraînement terminé. Retour au menu principal.|訓練完了。メインメニューへ戻る。
title.name|Loop Fighter|循环战机|Циклический истребитель|Caça em Loop|Schleifenjäger|Döngü Savaşçısı|Chasseur de boucle|ループ戦機
wake|WAKE|觉醒|ПРОБУЖДЕНИЕ|DESPERTAR|ERWACHEN|UYAN|ÉVEIL|覚醒
choir.core.event|THE CHOIR CORE|合唱核心|ЯДРО ХОРА|NÚCLEO DO CORO|CHORKERN|KORO ÇEKİRDEĞİ|NOYAU DU CHŒUR|合唱核
wave.intro|WAVE {0}|第 {0} 波|ВОЛНА {0}|ONDA {0}|WELLE {0}|DALGA {0}|VAGUE {0}|WAVE {0}
wave.engage|ENGAGE|接战|БОЙ|ENGAJAR|EINSATZ|ÇATIŞMA|ENGAGEMENT|交戦
sector.enter|SECTOR {0}: {1}|第 {0} 章：{1}|СЕКТОР {0}: {1}|SETOR {0}: {1}|SEKTOR {0}: {1}|BÖLGE {0}: {1}|SECTEUR {0}: {1}|セクター {0}: {1}
sector.cleared|{0} CLEARED|已突破：{0}|{0} ЗАЧИЩЕН|{0} LIMPO|{0} GESÄUBERT|{0} TEMİZLENDİ|{0} PURGÉ|{0} 突破
sector.0.name|Lumen Shoal|光滩星区|Отмель Люмена|Baixio Lúmen|Lumenriff|Lümen Sığlığı|Haut-fond Lumen|ルーメン浅瀬
sector.0.trait|Calm opening field. Learn your pilot weapon, skill, and red-fire pressure.|平稳的开场星域。熟悉武器、角色技与红弹压力。|Спокойный старт. Освой оружие, навык и давление красного огня.|Campo inicial calmo. Domine arma, habilidade e pressão vermelha.|Ruhiger Auftakt. Lerne Waffe, Skill und roten Beschuss.|Sakin başlangıç. Silahı, yeteneği ve kırmızı ateşi öğren.|Ouverture calme. Apprends arme, compétence et pression rouge.|穏やかな初域。武器、スキル、赤弾圧を覚えよう。
sector.1.name|Glass Reef|玻璃星礁|Стеклянный риф|Recife de Vidro|Glasriff|Cam Resifi|Récif de verre|ガラス礁
sector.1.trait|Warning beams cut across the arena before firing.|光束会先预警，再切开战场。|Луч сначала предупреждает, потом режет арену.|Feixes avisam antes de cortar a arena.|Strahlen warnen kurz, dann schneiden sie die Arena.|Işınlar önce uyarır, sonra arenayı keser.|Les rayons préviennent avant de fendre l’arène.|ビームは予告後に戦場を切り裂く。
sector.2.name|Verdant Grave|翠墓星域|Зеленая могила|Túmulo Verde|Grünes Grab|Yeşil Mezar|Tombe verdoyante|翠緑の墓域
sector.2.trait|Gravity wells begin to open. Step out before the core collapses.|重力场开始出现。看到圆形预警后，尽快离开核心区域。|Появляются гравитационные поля. Уходи из ядра до схлопывания.|Poços de gravidade surgem. Saia do núcleo antes do colapso.|Gravitationsfelder öffnen sich. Verlasse den Kern vor dem Kollaps.|Yerçekimi alanları açılır. Çekirdek çökmeden çık.|Des puits gravitaires s’ouvrent. Sors du cœur avant l’effondrement.|重力場が出現。円形警告を見たら中心から離れよう。
sector.3.name|Clock Cathedral|钟塔星域|Часовой собор|Catedral do Relógio|Uhrkathedrale|Saat Katedrali|Cathédrale-horloge|時計大聖堂
sector.3.trait|Time shears the arena. Faster enemies use readable dash lanes.|时间切割战场。高速敌人会用可预判的突进路线逼你走位。|Время режет арену. Быстрые враги делают читаемые рывки.|O tempo corta a arena. Inimigos rápidos avançam em linhas claras.|Zeit schert die Arena. Schnelle Feinde dashen auf lesbaren Bahnen.|Zaman arenayı keser. Hızlı düşmanlar okunur atılış yapar.|Le temps cisaille l’arène. Les ennemis rapides dashent sur des lignes lisibles.|時間が戦場を裂く。高速敵は読める突進線を使う。
sector.4.name|Solar Wound|太阳裂口|Солнечная рана|Ferida Solar|Sonnenwunde|Güneş Yarası|Plaie solaire|太陽裂傷
sector.4.trait|Final sector. Elite dashes, beams, and gravity fields overlap.|最终章。精英突进、光束和重力场会组合施压。|Финал. Рывки элиты, лучи и поля накладываются.|Setor final. Dashes elite, feixes e gravidade se cruzam.|Finale. Elite-Dashes, Strahlen und Felder überlagern sich.|Son bölge. Elit atılışlar, ışınlar ve alanlar üst üste gelir.|Secteur final. Dashes élite, rayons et champs se superposent.|最終域。精鋭突進、ビーム、重力場が重なる。
repair|REPAIR|修复|РЕМОНТ|REPARO|REPARATUR|ONARIM|RÉPARATION|修理
language.changed|LANGUAGE: ENGLISH|当前语言：中文|ЯЗЫК: РУССКИЙ|IDIOMA: PORTUGUÊS BR|SPRACHE: DEUTSCH|DİL: TÜRKÇE|LANGUE : FRANÇAIS|言語：日本語
language.hint|LANGUAGE: ENGLISH  [L]|中文  [L切换]|РУССКИЙ  [L]|PORTUGUÊS BR  [L]|DEUTSCH  [L]|TÜRKÇE  [L]|FRANÇAIS  [L]|日本語  [L]
menu.start|START GAME|开始游戏|НАЧАТЬ ИГРУ|INICIAR JOGO|SPIEL STARTEN|OYUNA BAŞLA|COMMENCER|ゲーム開始
menu.meta|FIGHTER BAY|战机升级室|АНГАР ИСТРЕБИТЕЛЯ|HANGAR DO CAÇA|JÄGERHANGAR|SAVAŞÇI HANGARI|HANGAR DU CHASSEUR|戦機格納庫
menu.language|SWITCH LANGUAGE|切换语言|СМЕНИТЬ ЯЗЫК|TROCAR IDIOMA|SPRACHE WECHSELN|DİL DEĞİŞTİR|CHANGER DE LANGUE|言語切替
menu.settings|SETTINGS|设置|НАСТРОЙКИ|CONFIGURAÇÕES|EINSTELLUNGEN|AYARLAR|PARAMÈTRES|設定
gm.unlock.label|GM|GM|GM|GM|GM|GM|GM|GM
gm.unlock.toast|All pilots and difficulties unlocked.|所有角色与难度已解锁。|Все пилоты и сложности открыты.|Pilotos e dificuldades liberados.|Alle Piloten und Stufen frei.|Tüm pilotlar ve zorluklar açık.|Pilotes et difficultés débloqués.|全パイロットと難易度解放。
difficulty.title|DIFFICULTY|难度|СЛОЖНОСТЬ|DIFICULDADE|SCHWIERIGKEIT|ZORLUK|DIFFICULTÉ|難易度
difficulty.cruise|Cruise|巡航|Крейсер|Cruzeiro|Cruise|Seyir|Croisière|巡航
difficulty.storm|Storm|风暴|Шторм|Tempestade|Sturm|Fırtına|Tempête|嵐
difficulty.eclipse|Eclipse|日蚀|Затмение|Eclipse|Finsternis|Tutulma|Éclipse|日蝕
difficulty.unlock|Clear {0} to unlock.|通关{0}解锁。|Пройди {0}, чтобы открыть.|Conclua {0} para liberar.|Schließe {0} ab.|{0} bitirerek aç.|Termine {0} pour débloquer.|{0}クリアで解放
menu.pilot|PILOT|角色|ПИЛОТ|PILOTO|PILOT|PİLOT|PILOTE|パイロット
pilot.selector.skill|Skill|技能|Навык|Habilidade|Skill|Yetenek|Compétence|スキル
pilot.selector.ultimate|Ultimate|大招|Ульта|Ultimate|Ultimativ|Ulti|Ultime|必殺
pilot.selector.selected|Selected|已选择|Выбран|Selecionado|Gewählt|Seçildi|Sélectionné|選択中
pilot.selector.locked|Locked|未解锁|Закрыт|Travado|Gesperrt|Kilitli|Verrouillé|ロック
menu.tip|Click START, press Enter, or press A. Spend Star Dust in Permanent Upgrades between runs.|点击开始、按 Enter 或 A。每局结束后用星尘强化永久节点。|Нажми старт, Enter или A. Между вылетами трать Звездную пыль на узлы.|Clique iniciar, Enter ou A. Gaste Pó Estelar em upgrades entre runs.|Klicke Start, Enter oder A. Gib Sternenstaub zwischen Runs aus.|Başlat, Enter veya A. Sefer aralarında Yıldız Tozu harca.|Clique, Enter ou A. Dépense la poussière stellaire entre deux runs.|開始、Enter、Aで出撃。星屑で恒久強化しよう。
ui.lock|LOCK|未解锁|ЗАКРЫТ|TRAVADO|GESPERRT|KİLİTLİ|VERROUILLÉ|ロック
ui.done|DONE|完成|ГОТОВО|FEITO|ERLEDIGT|TAMAM|FAIT|完了
boss.choir_core|CHOIR CORE|合唱核心|ЯДРО ХОРА|NÚCLEO DO CORO|CHORKERN|KORO ÇEKİRDEĞİ|NOYAU DU CHŒUR|合唱核
boss.sector|{0} CORE|{0} 核心|ЯДРО {0}|NÚCLEO {0}|{0}-KERN|{0} ÇEKİRDEĞİ|NOYAU {0}|{0}コア
boss.title|{0}: {1}|{0}：{1}|{0}: {1}|{0}: {1}|{0}: {1}|{0}: {1}|{0} : {1}|{0}: {1}
boss.preview|{0} BOSS VARIANT|{0} Boss 变体|БОСС {0}|CHEFE {0}|{0}-BOSSVARIANTE|{0} BOSS TÜRÜ|BOSS {0}|{0}ボス変種
boss.phase|PHASE {0}|阶段 {0}|ФАЗА {0}|FASE {0}|PHASE {0}|AŞAMA {0}|PHASE {0}|フェーズ {0}
boss.phase_hud|PHASE {0}/{1}|阶段 {0}/{1}|ФАЗА {0}/{1}|FASE {0}/{1}|PHASE {0}/{1}|AŞAMA {0}/{1}|PHASE {0}/{1}|フェーズ {0}/{1}
boss.guard_hud|GUARD {0}%|护盾 {0}%|ЗАЩИТА {0}%|GUARDA {0}%|SCHUTZ {0}%|KORUMA {0}%|GARDE {0}%|防御 {0}%
boss.next_hud|NEXT PHASE AT {0}%|下阶段 {0}%|СЛЕД. ФАЗА {0}%|PRÓX. FASE {0}%|NÄCHSTE PHASE {0}%|SONRAKİ {0}%|PROCHAINE {0}%|次フェーズ {0}%
boss.final_hud|FINAL PHASE|最终阶段|ФИНАЛЬНАЯ ФАЗА|FASE FINAL|FINALE PHASE|SON AŞAMA|PHASE FINALE|最終フェーズ
boss.intent_hud|MOVE {0}|招式 {0}|ПРИЕМ {0}|GOLPE {0}|ANGRIFF {0}|HAMLE {0}|COUP {0}|技 {0}
boss.pattern.fan|Fan|扇射|Веер|Leque|Fächer|Yelpaze|Éventail|扇
boss.pattern.spiral|Spiral|螺旋|Спираль|Espiral|Spirale|Sarmal|Spirale|螺旋
boss.pattern.lance|Lance|重枪|Копье|Lança|Lanze|Mızrak|Lance|槍
boss.pattern.summon|Summon|召援|Призыв|Invocar|Ruf|Çağrı|Appel|召喚
boss.pattern.hazard|Beam|光束|Луч|Feixe|Strahl|Işın|Rayon|ビーム
boss.pattern.reverse|Reverse|逆旋|Реверс|Reverso|Umkehr|Ters|Retour|逆転
boss.pattern.warden|Guard Call|护卫|Страж|Guarda|Wache|Muhafız|Garde|護衛
boss.pattern.cross|Cross|交叉|Крест|Cruz|Kreuz|Çapraz|Croix|十字
boss.pattern.mine|Mines|星雷|Мины|Minas|Minen|Mayın|Mines|機雷
boss.pattern.mirror|Mirror|镜像|Зеркало|Espelho|Spiegel|Ayna|Miroir|鏡
boss.pattern.tempest|Tempest|风暴|Буря|Tormenta|Sturm|Fırtına|Tempête|嵐
boss.pattern.bastion|Wall|壁垒|Стена|Muralha|Wall|Duvar|Mur|壁
boss.pattern.serpent|Coil|盘绕|Кольцо|Espiral|Winde|Kıvrım|Spire|渦
boss.pattern.oracle|Snipe|狙击|Снайп|Mira|Schuss|Nişan|Tir|狙撃
boss.choir.name|Choir Core|合唱核心|Ядро Хора|Núcleo do Coro|Chorkern|Koro Çekirdeği|Noyau du Chœur|合唱核
boss.prism.name|Prism Regent|棱镜王庭|Призменный регент|Regente Prisma|Prismenregent|Prizma Naibi|Régent prismatique|プリズム摂政
boss.swarm.name|Drone Matriarch|蜂群母舰|Матриарх дронов|Matriarca Drone|Drohnenmatriarchin|Sürü Anası|Matriarche drone|ドローン母巣
boss.forge.name|Solar Forge|日冕熔炉|Солнечная кузня|Forja Solar|Sonnenesse|Güneş Ocağı|Forge solaire|太陽炉
boss.rift.name|Rift Warden|裂隙典狱|Страж разлома|Carcereiro da Fenda|Risswächter|Yarık Gardiyanı|Gardien de faille|裂け目の番人
boss.choir.signature|Choir pulse|合唱脉冲|Пульс хора|Pulso coral|Chorpuls|Koro darbesi|Pouls chœur|合唱波
boss.prism.signature|Prism cross|棱镜交叉|Крест призмы|Cruz prisma|Prismenkreuz|Prizma çaprazı|Croix prisme|プリズム十字
boss.swarm.signature|Swarm call|蜂群召援|Зов роя|Chamado enxame|Schwarmruf|Sürü çağrısı|Appel essaim|群れ呼び
boss.forge.signature|Forge wall|熔炉壁垒|Стена кузни|Muro forja|Essewall|Ocak duvarı|Mur forge|炉壁
boss.rift.signature|Rift cut|裂隙切场|Разрез разлома|Corte fenda|Rissschnitt|Yarık kesiği|Fente faille|裂け目斬り
boss.mirror.signature|Mirror fork|镜像分叉|Зеркальная вилка|Forquilha espelho|Spiegelgabel|Ayna çatalı|Fourche miroir|鏡分岐
boss.tempest.signature|Tempest wheel|风暴轮转|Колесо бури|Roda tormenta|Sturmrad|Fırtına çarkı|Roue tempête|嵐の輪
boss.bastion.signature|Bastion wall|堡垒壁垒|Стена бастиона|Muro bastião|Bastionwall|Hisar duvarı|Mur bastion|要塞壁
boss.serpent.signature|Serpent coil|盘蛇绞环|Кольцо змея|Espiral serpente|Schlangenwinde|Yılan kıvrımı|Spire serpent|蛇の渦
boss.oracle.signature|Oracle mark|神谕狙击|Метка оракула|Marca oráculo|Orakelmarke|Kahin işareti|Marque oracle|神託狙撃
wave.enemy.focus|PRIMARY: {0} - {1}|主敌：{0} - {1}|ЦЕЛЬ: {0} - {1}|ALVO: {0} - {1}|PRIMÄR: {0} - {1}|ANA HEDEF: {0} - {1}|CIBLE : {0} - {1}|主目標：{0} - {1}
wave.enemy.support|SUPPORT: {0}|辅助：{0}|ПОДДЕРЖКА: {0}|APOIO: {0}|SUPPORT: {0}|DESTEK: {0}|SOUTIEN : {0}|支援：{0}
wave.enemy.new|NEW ENEMY: {0}|新敌人：{0}|НОВЫЙ ВРАГ: {0}|NOVO INIMIGO: {0}|NEUER FEIND: {0}|YENİ DÜŞMAN: {0}|NOUVEL ENNEMI : {0}|新敵：{0}
wave.intel.wave|WAVE {0}/{1}|波次 {0}/{1}|ВОЛНА {0}/{1}|ONDA {0}/{1}|WELLE {0}/{1}|DALGA {0}/{1}|VAGUE {0}/{1}|WAVE {0}/{1}
wave.intel.primary|MAIN {0}|主敌 {0}|ЦЕЛЬ {0}|ALVO {0}|ZIEL {0}|ANA {0}|CIBLE {0}|主敵 {0}
wave.intel.support|SUPPORT {0}|辅助 {0}|ПОДДЕРЖКА {0}|APOIO {0}|SUPPORT {0}|DESTEK {0}|SOUTIEN {0}|支援 {0}
wave.intel.incoming|LEFT {0}|剩余 {0}|ОСТАЛОСЬ {0}|RESTAM {0}|ÜBRIG {0}|KALAN {0}|RESTE {0}|残り {0}
wave.intel.progress|PROGRESS {0}%  LIVE {1}|进度 {0}%  场上 {1}|ПРОГРЕСС {0}%  В БОЮ {1}|PROGRESSO {0}%  VIVOS {1}|FORTSCHRITT {0}%  AKTIV {1}|İLERLEME {0}%  AKTİF {1}|PROGRESSION {0}%  ACTIFS {1}|進行 {0}%  出現 {1}
wave.intel.progress_short|{0}%  LIVE {1}|{0}%  场上 {1}|{0}%  В БОЮ {1}|{0}%  VIVOS {1}|{0}%  AKTIV {1}|{0}%  AKTİF {1}|{0}%  ACTIFS {1}|{0}%  出現 {1}
wave.intel.clear|SINGLE TYPE|单一敌群|ОДИН ТИП|TIPO ÚNICO|EIN TYP|TEK TÜR|TYPE UNIQUE|単一種
wave.intel.batch|NEXT {0:0.0}s  x{1}|下一批 {0:0.0}秒  x{1}|СЛЕД. {0:0.0}с  x{1}|PRÓX. {0:0.0}s  x{1}|NÄCHSTE {0:0.0}s  x{1}|SONRA {0:0.0}sn  x{1}|PROCH. {0:0.0}s  x{1}|次 {0:0.0}秒  x{1}
wave.intel.reserve|POOL {0}|储备 {0}|ПУЛ {0}|RESERVA {0}|POOL {0}|HAVUZ {0}|RÉSERVE {0}|残数 {0}
wave.intel.complete|POOL EMPTY|储备清空|ПУЛ ПУСТ|SEM RESERVA|POOL LEER|HAVUZ BOŞ|RÉSERVE VIDE|残数なし
wave.pace.standard|PATROL|巡逻|ПАТРУЛЬ|PATRULHA|PATROUILLE|DEVRİYE|PATROUILLE|哨戒
wave.pace.swarm|SWARM|蜂群|РОЙ|ENXAME|SCHWARM|SÜRÜ|ESSAIM|群襲
wave.pace.elite|ELITE HUNT|精英猎场|ОХОТА ЭЛИТЫ|CAÇADA ELITE|ELITEJAGD|ELİT AVI|CHASSE ÉLITE|精鋭狩り
wave.pace.recovery|SUPPLY DRIFT|补给漂流|ДРЕЙФ СНАБЖЕНИЯ|SUPRIMENTO|VERSORGUNG|İKMAL AKINTISI|DÉRIVE DE RAVITAILLEMENT|補給流
wave.pace.pressure|PRESSURE RUN|压迫连战|ДАВЛЕНИЕ|PRESSÃO|DRUCKLAUF|BASKI KOŞUSU|RUÉE SOUS PRESSION|圧力戦
wave.pace.boss|BOSS|Boss|БОСС|CHEFE|BOSS|BOSS|BOSS|ボス
wave.pace.short.standard|PATROL|巡逻|ПАТРУЛЬ|PATRULHA|PATROUILLE|DEVRİYE|PATROUILLE|哨戒
wave.pace.short.swarm|SWARM|蜂群|РОЙ|ENXAME|SCHWARM|SÜRÜ|ESSAIM|群襲
wave.pace.short.elite|ELITE|精英|ЭЛИТА|ELITE|ELITE|ELİT|ÉLITE|精鋭
wave.pace.short.recovery|SUPPLY|补给|СНАБЖ.|SUPRIM.|VERSORG.|İKMAL|RAVIT.|補給
wave.pace.short.pressure|RUSH|压迫|РЫВОК|PRESSÃO|DRUCK|BASKI|RUÉE|圧力
wave.pace.short.boss|BOSS|Boss|БОСС|CHEFE|BOSS|BOSS|BOSS|ボス
flow.momentum|MOMENTUM +FOCUS +ENERGY|连战动量：专注与能量提升|ТЕМП +ФОКУС +ЭНЕРГИЯ|RITMO +FOCO +ENERGIA|MOMENTUM +FOKUS +ENERGIE|İVME +ODAK +ENERJİ|ÉLAN +FOCUS +ÉNERGIE|勢い +集中 +エネルギー
flow.supply|SUPPLY WAVE: fewer enemies, better recovery.|补给波：敌人较少，恢复更多。|Волна снабжения: меньше врагов, больше восстановления.|Onda de suprimento: menos inimigos, mais cura.|Versorgung: weniger Feinde, bessere Erholung.|İkmal dalgası: az düşman, çok toparlanma.|Ravitaillement : moins d’ennemis, meilleure récupération.|補給波：敵少なめ、回復多め。
flow.draft|BUILD DRAFT READY|构筑选择就绪|ВЫБОР СБОРКИ ГОТОВ|ESCOLHA DE BUILD|BUILD-WAHL BEREIT|YAPI SEÇİMİ HAZIR|CHOIX DE BUILD PRÊT|ビルド選択可能
flow.reward.clean|No damage: next wave reward improves.|无伤通过：下一波奖励提高。|Без урона: награда следующей волны выше.|Sem dano: próxima recompensa melhora.|Ohne Schaden: nächste Belohnung steigt.|Hasarsız: sonraki ödül artar.|Sans dégâts : prochaine récompense accrue.|無傷：次Wave報酬上昇。
flow.reward.swarm|Swarm routed: extra EXP crystals drift in.|蜂群击溃：额外经验晶体出现。|Рой разбит: плывут кристаллы опыта.|Enxame vencido: cristais EXP surgem.|Schwarm gebrochen: EXP-Kristalle treiben ein.|Sürü dağıldı: EXP kristalleri gelir.|Essaim brisé : cristaux EXP en plus.|群襲撃破：追加EXP結晶。
flow.reward.elite|Elite broken: next wave weapon pressure rises.|精英破阵：下一波武器压力提高。|Элита сломлена: оружие сильнее в следующей волне.|Elite quebrada: pressão da arma sobe.|Elite gebrochen: Waffendruck steigt.|Elit kırıldı: silah baskısı artar.|Élite brisée : pression d’arme accrue.|精鋭撃破：次波の火力上昇。
flow.reward.recovery|Supply secured: hull and energy restored.|补给稳固：生命与能量恢复。|Снабжение взято: корпус и энергия восстановлены.|Suprimento seguro: casco e energia restaurados.|Versorgung gesichert: Hülle und Energie zurück.|İkmal alındı: gövde ve enerji yenilenir.|Ravitaillement : coque et énergie restaurées.|補給確保：耐久とエネルギー回復。
flow.reward.pressure|Pressure held: next wave opens with advantage.|压迫顶住：下一波获得先手。|Давление выдержано: следующая волна с преимуществом.|Pressão segurada: próxima onda começa melhor.|Druck gehalten: nächste Welle mit Vorteil.|Baskı tutuldu: sonraki dalga avantajlı.|Pression tenue : avantage à la vague suivante.|圧力突破：次波を有利に開始。
flow.event.elite|Momentum event: elite target deployed.|连击事件：精英目标出现。|Событие темпа: элитная цель.|Evento de ritmo: elite enviada.|Momentum: Eliteziel entsandt.|İvme olayı: elit hedef geldi.|Événement d’élan : cible élite.|勢いイベント：精鋭出現。
flow.event.supply|Momentum event: field supply released.|连击事件：战场补给释放。|Событие темпа: снабжение.|Evento de ritmo: suprimento liberado.|Momentum: Versorgung frei.|İvme olayı: ikmal düştü.|Événement d’élan : ravitaillement.|勢いイベント：補給放出。
flow.event.surge|Wave surge: reserve group entering.|波中涌入：储备敌群进入。|Всплеск волны: резерв входит.|Surto: reserva entrando.|Wellenschub: Reserve kommt.|Dalga atağı: yedek grup geliyor.|Sursaut : réserve en approche.|波中突入：予備群接近。
enemy.chaser.name|Chaser|追猎者|Гонщик|Caçador|Jäger|Takipçi|Traqueur|追跡者
enemy.chaser.role|rushes you and fires simple aimed shots|直线追击并发射瞄准弹|сближается и стреляет прицельно|avança e atira mirando|stürmt und feuert gezielt|üstüne gelir ve nişan alır|fonce et tire droit|突進して狙撃する
enemy.weaver.name|Weaver|织弹者|Ткач|Tecelão|Weber|Örücü|Tisseur|織弾者
enemy.weaver.role|moves sideways and fires small fans|横移并打出小扇形弹幕|ходит боком и веером стреляет|anda de lado e abre leques|flankiert und feuert Fächer|yana kayar ve yelpaze atar|se décale et tire en éventail|横移動し扇状弾
enemy.turret.name|Turret|炮台|Турель|Torreta|Geschütz|Taret|Tourelle|砲台
enemy.turret.role|keeps distance and creates ring patterns|保持距离并制造环形弹幕|держит дистанцию и строит кольца|mantém distância e cria anéis|hält Abstand und baut Ringe|uzakta durur, halka kurar|reste loin et trace des anneaux|距離を取り環状弾
enemy.splitter.name|Splitter|分裂体|Делитель|Divisor|Splitter|Bölücü|Diviseur|分裂体
enemy.splitter.role|breaks into smaller attackers when destroyed|被击破后分裂成小敌人|после гибели делится на мелких врагов|ao morrer vira inimigos menores|zerfällt in kleine Angreifer|ölünce küçük düşmanlara ayrılır|se brise en petits assaillants|撃破時に小型化
enemy.lance.name|Lance|长枪手|Копье|Lança|Lanze|Mızrak|Lance|槍兵
enemy.lance.role|fires fast heavy shots from long range|远距离发射高速重弹|издали бьет тяжелыми снарядами|atira pesado de longe|feuert schwere Fernschüsse|uzaktan ağır atış yapar|tire lourd à longue portée|遠距離の重弾
enemy.mine.name|Mine|星雷|Мина|Mina|Mine|Mayın|Mine|機雷
enemy.mine.role|drifts slowly and bursts into radial bullets|缓慢漂移并爆成放射弹|медленно дрейфует и взрывается кругом|deriva e explode em anel|driftet und platzt radial|süzülür, çember saçar|dérive puis éclate en cercle|漂流して全方位弾
enemy.shard.name|Shard|碎片|Осколок|Estilhaço|Splitter|Kıymık|Éclat|破片
enemy.shard.role|fast harasser with quick paired shots|高速骚扰并连射成对小弹|быстрый враг с парными выстрелами|rápido e dispara em dupla|schneller Störer mit Doppelschüssen|hızlı tacizci, çift atış|harceleur rapide à tirs doubles|高速の二連射
enemy.warden.name|Warden|守望者|Надзиратель|Guardião|Wächter|Muhafız|Gardien|番人
enemy.warden.role|summons support units while suppressing space|召唤援兵并压制空间|зовет поддержку и давит пространство|invoca apoio e trava espaço|ruft Hilfe und sperrt Raum|destek çağırır, alan kapatır|invoque et verrouille l’espace|増援で場を制圧
enemy.drifter.name|Drifter|弧行者|Скиталец|Derivante|Drifter|Süzülen|Dériveur|漂流者
enemy.drifter.role|curves around you and shoots from angles|绕弧移动并从侧面射击|обходит дугой и стреляет сбоку|faz curva e atira de ângulos|umkreist und schießt schräg|yay çizip açıdan vurur|tourne et tire en angle|弧を描き側面射撃
enemy.bulwark.name|Bulwark|壁垒|Оплот|Baluarte|Bollwerk|Siper|Rempart|防壁
enemy.bulwark.role|slow armored target that anchors the wave|缓慢高血量，是本波核心目标|медленная броня, якорь волны|tanque lento que segura a onda|langsamer Panzer als Wellenanker|yavaş zırhlı dalga direği|cible lente et blindée|鈍重な高耐久目標
enemy.siren.name|Siren|鸣标|Сирена|Sirene|Sirene|Siren|Sirène|サイレン
enemy.siren.role|cycles its core and releases slow red rings|蓄能后释放慢速红环|заряжает ядро и пускает красные кольца|carrega e solta anéis vermelhos|lädt und sendet rote Ringe|yüklenir, kırmızı halka salar|charge puis lance des anneaux rouges|赤い輪を放つ
enemy.harrier.name|Harrier|掠袭者|Гарпия|Assaltante|Harrier|Akıncı|Harceleur|急襲機
enemy.harrier.role|dives quickly, then fires short bursts|快速突进，停顿后短促连射|ныряет и дает короткую очередь|mergulha e dispara rajadas|stürzt und feuert Salven|dalıp kısa seri atar|plonge puis rafale|突進後に短射撃
hud.hull|HULL|生命|КОРПУС|CASCO|HÜLLE|GÖVDE|COQUE|耐久
hud.energy|ENERGY|能量|ЭНЕРГИЯ|ENERGIA|ENERGIE|ENERJİ|ÉNERGIE|エネルギー
hud.dash|DASH|冲刺|РЫВОК|DASH|SPRINT|ATIL|DASH|ダッシュ
hud.settings|SETTINGS|设置|НАСТР.|AJUSTES|OPTIONEN|AYAR|PARAM.|設定
hud.wave|WAVE {0}/40|波次 {0}/40|ВОЛНА {0}/40|ONDA {0}/40|WELLE {0}/40|DALGA {0}/40|VAGUE {0}/40|WAVE {0}/40
hud.sector|SECTOR {0}/5|章节 {0}/5|СЕКТОР {0}/5|SETOR {0}/5|SEKTOR {0}/5|BÖLGE {0}/5|SECTEUR {0}/5|セクター {0}/5
hud.wave.label|WAVE|波次|ВОЛНА|ONDA|WELLE|DALGA|VAGUE|WAVE
hud.sector.label|SECTOR|章节|СЕКТОР|SETOR|SEKTOR|BÖLGE|SECTEUR|区域
hud.route.label|ROUTE|路线|МАРШРУТ|ROTA|ROUTE|ROTA|ROUTE|進路
hud.spawn.label|SPAWN|刷新|СПАВН|SPAWN|SPAWN|DOĞUŞ|APPAR.|出現
hud.spawn.combo|CD -{0}%|减CD {0}%|КД -{0}%|CD -{0}%|CD -{0}%|CD -{0}%|CD -{0}%|CD -{0}%
hud.spawn.next_short|{0:0.0}s x{1}|{0:0.0}秒 x{1}|{0:0.0}с x{1}|{0:0.0}s x{1}|{0:0.0}s x{1}|{0:0.0}sn x{1}|{0:0.0}s x{1}|{0:0.0}秒 x{1}
hud.spawn.done|CLEAR|清空|ПУСТО|LIMPO|LEER|TEMİZ|VIDE|完了
hud.progress.label|PACE|推进|ТЕМП|RITMO|TEMPO|TEMPO|RYTHME|進行
hud.progress.combo|PACE +{0}%|推进 +{0}%|ТЕМП +{0}%|RITMO +{0}%|TEMPO +{0}%|TEMPO +{0}%|RYTHME +{0}%|進行 +{0}%
hud.spawn.boss|--|--|--|--|--|--|--|--
hud.xp.label|EXP|经验|ОПЫТ|EXP|EP|TP|EXP|EXP
hud.level.label|LV|等级|УР|NV|ST|SV|NV|Lv
hud.cache.label|CACHE|缓存|ЗАПАС|CACHE|CACHE|ÖNBELLEK|CACHE|蓄積
hud.combo.label|COMBO|连击|КОМБО|COMBO|KOMBO|KOMBO|COMBO|コンボ
combo.pop|COMBO x{0}|连击X{0}|КОМБО x{0}|COMBO x{0}|KOMBO x{0}|KOMBO x{0}|COMBO x{0}|コンボ x{0}
combo.value|x{0}|连击X{0}|x{0}|x{0}|x{0}|x{0}|x{0}|x{0}
combo.surge|COMBO SURGE x{0}|连击X{0} 加速|КОМБО РЫВОК x{0}|SURTO x{0}|KOMBO-SCHUB x{0}|KOMBO AKIŞI x{0}|POUSSÉE x{0}|コンボ加速 x{0}
hud.pace.label|PACE|节奏|ТЕМП|RITMO|TEMPO|TEMPO|RYTHME|速度
hud.build|BUILD|构筑|СБОРКА|BUILD|BUILD|YAPI|BUILD|ビルド
hud.objectives|EXPEDITION GOALS|远征目标|ЦЕЛИ ВЫЛЕТА|METAS DA EXPEDIÇÃO|EXPEDITIONSZIELE|SEFER HEDEFLERİ|OBJECTIFS D’EXPÉDITION|遠征目標
hud.cyan_resonance|TACTICAL|战术技|ТАКТИКА|TÁTICO|TAKTIK|TAKTİK|TACTIQUE|戦術
hud.gold_resonance|TACTICAL|战术技|ТАКТИКА|TÁTICO|TAKTIK|TAKTİK|TACTIQUE|戦術
hud.resonance_ready|SKILL READY|技能就绪|НАВЫК ГОТОВ|HABILIDADE PRONTA|SKILL BEREIT|YETENEK HAZIR|COMPÉTENCE PRÊTE|スキル準備完了
hud.resonance_cooldown|COOLDOWN {0:0.0}s|冷却 {0:0.0}秒|ОТКАТ {0:0.0}с|RECARGA {0:0.0}s|ABKLINGZEIT {0:0.0}s|BEKLEME {0:0.0}sn|RECHARGE {0:0.0}s|CD {0:0.0}秒
hud.cruise_charge|FOCUS|专注|ФОКУС|FOCO|FOKUS|ODAK|FOCUS|集中
hud.assault_window|SKILL {0:0.0}s|战术 {0:0.0}秒|НАВЫК {0:0.0}с|HABIL. {0:0.0}s|SKILL {0:0.0}s|YETENEK {0:0.0}sn|COMP. {0:0.0}s|スキル {0:0.0}秒
hud.controls|WASD/LS MOVE  MOUSE/RS AIM DASH  AUTO FIRE  A/LB DASH  X/RB SKILL  Y/RT ULT|WASD/左摇杆移动  鼠标/右摇杆定冲刺  自动开火  A/LB冲刺  X/RB技能  Y/RT大招|WASD/LS ДВИЖ.  МЫШЬ/RS РЫВОК  АВТООГОНЬ  A/LB РЫВОК  X/RB НАВЫК  Y/RT УЛЬТ|WASD/LS MOVER  MOUSE/RS MIRA DASH  AUTO  A/LB DASH  X/RB HABIL.  Y/RT ULT|WASD/LS BEWEGEN  MAUS/RS DASH-ZIEL  AUTOFEUER  A/LB SPRINT  X/RB SKILL  Y/RT ULT|WASD/LS HAREKET  MOUSE/RS ATIL YÖNÜ  OTO ATEŞ  A/LB ATIL  X/RB YETENEK  Y/RT ULTI|WASD/LS BOUGER  SOURIS/RS VISE DASH  AUTO  A/LB DASH  X/RB COMP.  Y/RT ULT|WASD/LS移動  マウス/RSでダッシュ方向  自動射撃  A/LBダッシュ  X/RBスキル  Y/RT奥義
xp.gain|+{0} EXP|+{0} 经验|+{0} ОПЫТА|+{0} EXP|+{0} EP|+{0} TP|+{0} EXP|+{0} EXP
xp.level_up|LEVEL UP|升级|УРОВЕНЬ +|SUBIU NÍVEL|LEVEL AUF|SEVİYE +|NIVEAU +|レベルアップ
score.cache|SCORE CACHE +{0} DUST|战绩缓存 +{0} 星尘|ЗАПАС +{0} ПЫЛИ|CACHE +{0} PÓ|CACHE +{0} STAUB|ÖDÜL +{0} TOZ|CACHE +{0} POUSSIÈRE|蓄積 +{0} 星屑
score.combo_break|COMBO BREAK|连击中断|КОМБО СБИТО|COMBO QUEBRADO|KOMBO BRICHT|KOMBO BOZULDU|COMBO BRISÉ|コンボ途切れ
title.subtitle|a pilot-build arcade roguelite built in Godot C#|Godot C# 制作的角色构筑弹幕 Roguelite|аркадный roguelite о пилотах и сборках|roguelite arcade de pilotos e builds|Pilot-Build-Arcade-Roguelite in Godot C#|pilot yapı odaklı arcade roguelite|roguelite arcade de pilote et de build|パイロットビルド型ローグライト
title.body|Dodge red bullets, collect EXP, and choose upgrades. Each pilot has a different weapon and skill. Ultimate clears nearby bullets.|躲红弹、吃经验、选升级。每个角色都有不同武器和技能。大招清除身边红弹。|Уклоняйся от красных пуль, собирай опыт и выбирай улучшения. У пилотов разное оружие и навык. Ульта чистит пули рядом.|Desvie de tiros vermelhos, colete EXP e escolha upgrades. Cada piloto tem arma e habilidade próprias. Ultimate limpa tiros perto.|Weiche roten Kugeln aus, sammle EP und wähle Upgrades. Jeder Pilot hat eigene Waffe und Skill. Ult räumt Kugeln nah bei dir.|Kırmızı mermilerden kaç, EXP topla ve geliştirme seç. Her pilotun silahı ve yeteneği farklı. Ulti yakındaki mermileri temizler.|Esquive les tirs rouges, ramasse l’EXP et choisis tes améliorations. Chaque pilote a son arme et sa compétence. L’ultime nettoie les tirs proches.|赤弾を避け、EXPを集め、強化を選ぶ。各機は武器とスキルが違う。奥義は近くの赤弾を消す。
title.start|ENTER / CLICK / A|ENTER / 点击 / A 开始|ENTER / КЛИК / A|ENTER / CLIQUE / A|ENTER / KLICK / A|ENTER / TIKLA / A|ENTRÉE / CLIC / A|ENTER / クリック / A
title.won_once|Choir Core fractured once. It remembers.|合唱核心已被击碎一次。它记得你。|Ядро Хора уже треснуло. Оно помнит.|O Núcleo já rachou. Ele lembra.|Der Chorkern brach einmal. Er erinnert sich.|Koro Çekirdeği çatladı. Seni hatırlar.|Le Noyau s’est fissuré. Il se souvient.|合唱核は一度砕けた。覚えている。
pilot.astra.name|Astra|星棱|Астра|Astra|Astra|Astra|Astra|アストラ
pilot.astra.body|Balanced straight shots. Skill fires a piercing beam at the locked target.|均衡直线射击。技能向锁定目标打出穿透光束。|Ровные прямые выстрелы. Навык бьет лучом по захваченной цели.|Tiros retos equilibrados. A habilidade dispara um feixe no alvo travado.|Ausgewogene Geradeaus-Schüsse. Der Skill feuert einen Strahl auf das erfasste Ziel.|Dengeli düz atış. Yetenek kilitli hedefe delici ışın atar.|Tirs droits équilibrés. La compétence tire un rayon sur la cible verrouillée.|安定した直線射撃。スキルはロック対象へ貫通ビームを撃つ。
pilot.astra.weapon|Prism Bolts|棱镜连射|Призменные болты|Dardos Prisma|Prismenbolzen|Prizma Mermileri|Traits prismatiques|プリズム弾
pilot.vesper.name|Vesper|暮轨|Веспер|Vesper|Vesper|Vesper|Vesper|ヴェスパー
pilot.vesper.body|Slow high-damage piercing shots. Skill creates a long damage line.|射速慢、伤害高、可穿透。技能生成一条长伤害线。|Медленные пробивающие выстрелы с высоким уроном. Навык создает длинную линию урона.|Tiros lentos, fortes e perfurantes. A habilidade cria uma linha longa de dano.|Langsame, starke Durchschüsse. Der Skill erzeugt eine lange Schadenslinie.|Yavaş, güçlü ve delici atış. Yetenek uzun hasar çizgisi açar.|Tirs lents, forts et perforants. La compétence crée une longue ligne de dégâts.|低速高威力の貫通弾。スキルは長いダメージラインを作る。
pilot.vesper.weapon|Rail Lance|轨道长枪|Рельсовое копье|Lança de Trilho|Schienenlanze|Ray Mızrağı|Lance-rail|レールランス
pilot.kairo.name|Kairo|环序|Кайро|Kairo|Kairo|Kairo|Kairo|カイロ
pilot.kairo.body|Starts with drones. Skill recalls drones, clears nearby bullets, and fires together.|开局带无人机。技能召回无人机、清近身红弹，并一起开火。|Начинает с дронами. Навык отзывает дронов, чистит пули рядом и стреляет вместе.|Começa com drones. A habilidade chama drones, limpa tiros perto e atira junto.|Startet mit Drohnen. Der Skill ruft Drohnen zurück, räumt nahe Kugeln und feuert zusammen.|Dronlarla başlar. Yetenek dronları çağırır, yakın mermiyi temizler ve birlikte ateş eder.|Commence avec des drones. La compétence les rappelle, nettoie les tirs proches et tire avec eux.|ドローン開始。スキルで戻し、近くの弾を消し、一斉射撃。
pilot.kairo.weapon|Drone Net|无人机网|Сеть дронов|Rede Drone|Drohnennetz|Dron Ağı|Réseau drone|ドローン網
pilot.sol.name|Sol|日冕|Сол|Sol|Sol|Sol|Sol|ソル
pilot.sol.body|Wide spread shots. Skill clears bullets, heals, and hits nearby enemies.|宽角散射。技能清弹、回血，并伤害附近敌人。|Широкий веер. Навык чистит пули, лечит и бьет врагов рядом.|Disparo aberto. A habilidade limpa tiros, cura e atinge inimigos perto.|Breite Streuschüsse. Der Skill räumt Kugeln, heilt und trifft nahe Feinde.|Geniş saçma atışı. Yetenek mermi temizler, iyileştirir ve yakını vurur.|Tirs en éventail. La compétence nettoie, soigne et touche les ennemis proches.|広角散弾。スキルは弾消し、回復、近くの敵へ攻撃。
pilot.sol.weapon|Corona Scatter|日冕散射|Коронный веер|Dispersão Corona|Koronastreuung|Korona Saçması|Dispersion corona|コロナ散弾
pilot.unlock.free|Unlocked|已解锁|Открыт|Desbloqueado|Freigeschaltet|Açık|Déverrouillé|解放済み
pilot.unlock.chain|Play one run with {0} {1}/{2}|用 {0} 出航一次 {1}/{2}|Сыграй вылет за {0} {1}/{2}|Jogue uma run com {0} {1}/{2}|Spiele einen Run mit {0} {1}/{2}|{0} ile bir sefer oyna {1}/{2}|Joue une run avec {0} {1}/{2}|{0}で1回出撃 {1}/{2}
pilot.unlock.vesper|Kills {0}/{1} · Wave {2}/{3}|击杀 {0}/{1} · 波次 {2}/{3}|Убийства {0}/{1} · волна {2}/{3}|Abates {0}/{1} · onda {2}/{3}|Kills {0}/{1} · Welle {2}/{3}|Öldürme {0}/{1} · dalga {2}/{3}|Élim. {0}/{1} · vague {2}/{3}|撃破 {0}/{1} · Wave {2}/{3}
pilot.unlock.kairo|Drops {0}/{1} · Runs {2}/{3} · Wave {4}/{5}|拾取 {0}/{1} · 出航 {2}/{3} · 波次 {4}/{5}|Дроп {0}/{1} · вылеты {2}/{3} · волна {4}/{5}|Drops {0}/{1} · runs {2}/{3} · onda {4}/{5}|Drops {0}/{1} · Runs {2}/{3} · Welle {4}/{5}|Düşen {0}/{1} · sefer {2}/{3} · dalga {4}/{5}|Butin {0}/{1} · runs {2}/{3} · vague {4}/{5}|収集 {0}/{1} · 出撃 {2}/{3} · Wave {4}/{5}
pilot.unlock.sol|Boss {0}/{1} · Clean {2}/{3} · Wave {4}/{5}|Boss {0}/{1} · 无伤波 {2}/{3} · 波次 {4}/{5}|Боссы {0}/{1} · чисто {2}/{3} · волна {4}/{5}|Chefes {0}/{1} · limpas {2}/{3} · onda {4}/{5}|Bosse {0}/{1} · sauber {2}/{3} · Welle {4}/{5}|Boss {0}/{1} · temiz {2}/{3} · dalga {4}/{5}|Boss {0}/{1} · parfait {2}/{3} · vague {4}/{5}|ボス {0}/{1} · 無傷 {2}/{3} · Wave {4}/{5}
ultimate.astra|PRISM NOVA|棱镜星爆|ПРИЗМЕННАЯ НОВА|NOVA PRISMA|PRISMENNOVA|PRİZMA NOVA|NOVA PRISME|プリズムノヴァ
ultimate.vesper|RAIL JUDGMENT|轨道裁决|РЕЛЬСОВЫЙ СУД|JULGAMENTO RAIL|SCHIENENGERICHT|RAY HÜKMÜ|JUGEMENT RAIL|レール審判
ultimate.kairo|SWARM OVERRIDE|蜂群覆写|ПЕРЕХВАТ РОЯ|SOBRESCRITA DO ENXAME|SCHWARM-OVERRIDE|SÜRÜ GEÇERSİZ KILMA|SURCHARGE D’ESSAIM|群制御上書き
ultimate.sol|CORONA FLARE|日冕耀斑|КОРОННАЯ ВСПЫШКА|CLARÃO CORONA|KORONENFLARE|KORONA PARLAMASI|ÉRUPTION CORONA|コロナフレア
ultimate.common|EMERGENCY CLEAR|紧急清弹|ЭКСТРЕННАЯ ОЧИСТКА|LIMPEZA DE EMERGÊNCIA|NOTRÄUMUNG|ACİL TEMİZLEME|NETTOYAGE URGENT|緊急弾消し
ultimate.cooldown|Ultimate cooldown {0}s|大招冷却 {0} 秒|Откат ульты {0}с|Ultimate em recarga {0}s|Ult-Abklingzeit {0}s|Ulti bekleme {0}sn|Ultime en recharge {0}s|奥義CD {0}秒
ultimate.need_energy|Need {0} energy|需要 {0} 能量|Нужно {0} энергии|Precisa de {0} energia|Benötigt {0} Energie|{0} enerji gerekli|Il faut {0} énergie|エネルギー {0} 必要
tactical.astra.name|PRISM FOCUS|棱镜聚焦|ПРИЗМА-ФОКУС|FOCO PRISMA|PRISMENFOKUS|PRİZMA ODAĞI|FOCUS PRISME|プリズム集中
tactical.vesper.name|RAIL LOCK|轨道锁定|РЕЛЬСОВЫЙ ЗАМОК|TRAVA RAIL|SCHIENENLOCK|RAY KİLİDİ|VERROU RAIL|レールロック
tactical.kairo.name|SWARM ORDER|蜂群指令|ПРИКАЗ РОЮ|ORDEM DO ENXAME|SCHWARMBEFEHL|SÜRÜ EMRİ|ORDRE D’ESSAIM|群体指令
tactical.sol.name|CORONA FIELD|日冕场|КОРОННОЕ ПОЛЕ|CAMPO CORONA|KORONENFELD|KORONA ALANI|CHAMP CORONA|コロナ場
tactical.astra.tip|Piercing beam toward the locked target.|向锁定目标打出穿透光束。|Пробивающий луч по захваченной цели.|Feixe perfurante no alvo travado.|Piercing-Strahl auf das erfasste Ziel.|Kilitli hedefe delici ışın.|Rayon perforant sur la cible verrouillée.|ロック対象へ貫通ビーム。
tactical.vesper.tip|Long damage line toward the locked target.|向锁定目标生成长伤害线。|Длинная линия урона к захваченной цели.|Linha longa de dano no alvo travado.|Lange Schadenslinie zum erfassten Ziel.|Kilitli hedefe uzun hasar çizgisi.|Longue ligne de dégâts vers la cible verrouillée.|ロック対象へ長いダメージ線。
tactical.kairo.tip|Drones fire together and clear nearby bullets.|无人机一起开火，并清除近身红弹。|Дроны стреляют вместе и чистят пули рядом.|Drones atiram juntos e limpam tiros perto.|Drohnen feuern zusammen und räumen nahe Kugeln.|Dronlar birlikte ateş eder ve yakın mermiyi temizler.|Les drones tirent ensemble et nettoient les tirs proches.|ドローンが一斉射撃し、近くの弾を消す。
tactical.sol.tip|Clear bullets, heal, and hit nearby enemies.|清弹、回血，并伤害附近敌人。|Чистит пули, лечит и бьет врагов рядом.|Limpa tiros, cura e acerta inimigos perto.|Räumt Kugeln, heilt und trifft nahe Feinde.|Mermi temizler, iyileştirir ve yakını vurur.|Nettoie les tirs, soigne et touche autour.|弾消し、回復、近くの敵へ攻撃。
tactical.cooldown|SKILL COOLDOWN {0}s|技能冷却 {0} 秒|ОТКАТ НАВЫКА {0}с|RECARGA {0}s|SKILL-COOLDOWN {0}s|YETENEK BEKLEME {0}sn|RECHARGE {0}s|スキルCD {0}秒
tactical.focus|FOCUS|专注|ФОКУС|FOCO|FOKUS|ODAK|FOCUS|集中
tactical.clear|CLEAR +{0}|清弹 +{0}|ОЧИСТКА +{0}|LIMPEZA +{0}|RÄUMEN +{0}|TEMİZLE +{0}|NETTOYAGE +{0}|弾消し +{0}
tactical.overheat|OVERHEAT BREAK|过热破绽|ПЕРЕГРЕВ СЛОМАН|QUEBRA DE CALOR|ÜBERHITZUNG BRICHT|AŞIRI ISI KIRILDI|SURCHAUFFE BRISÉE|過熱ブレイク
objective.bonus|BONUS +{0}|奖励 +{0}|БОНУС +{0}|BÔNUS +{0}|BONUS +{0}|BONUS +{0}|BONUS +{0}|ボーナス +{0}
objective.reach_wave|Reach wave {0}|到达第 {0} 波|Дойти до волны {0}|Chegar à onda {0}|Welle {0} erreichen|{0}. dalgaya ulaş|Atteindre vague {0}|Wave {0} 到達
objective.perfect_waves|Clear {0} waves without damage|无伤通过 {0} 波|Пройти {0} волн без урона|Passar {0} ondas sem dano|{0} Wellen ohne Schaden|Hasarsız {0} dalga geç|Finir {0} vagues sans dégâts|無傷で {0} Wave クリア
objective.defeat_enemies|Defeat {0} enemies|击败敌人 {0} 个|Уничтожить врагов: {0}|Derrotar {0} inimigos|{0} Feinde besiegen|{0} düşman yok et|Tuer {0} ennemis|敵撃破 {0}
objective.absorb_bullets|Graze or clear {0} red bullets|擦弹或清除 {0} 个红弹|Задеть или очистить {0} красных пуль|Raspar ou limpar {0} tiros vermelhos|{0} rote Kugeln streifen oder räumen|{0} kırmızı mermi sıyır veya temizle|Frôler ou nettoyer {0} tirs rouges|赤弾を {0} 回グレイズ/消去
objective.collect_pickups|Collect {0} EXP blocks|拾取 {0} 个经验块|Собрать {0} блоков опыта|Coletar {0} blocos de EXP|{0} EP-Blöcke sammeln|{0} EXP bloğu topla|Ramasser {0} blocs d’EXP|EXPブロック {0} 個回収
objective.best_combo|Reach combo X{0}|达到连击X{0}|Достичь комбо X{0}|Chegar ao combo X{0}|Kombo X{0} erreichen|X{0} kombo yap|Atteindre combo X{0}|コンボX{0} 到達
objective.defeat_bosses|Defeat {0} bosses|击败 Boss {0} 个|Победить боссов: {0}|Derrotar {0} chefes|{0} Bosse besiegen|{0} boss yen|Vaincre {0} boss|ボス撃破 {0}
objective.default|Complete goal|完成目标|Выполнить цель|Completar meta|Ziel erfüllen|Hedefi tamamla|Terminer l’objectif|目標達成
objective.complete|GOAL DONE +{0} DUST|目标完成 +{0} 星尘|ЦЕЛЬ ГОТОВА +{0} ПЫЛИ|META FEITA +{0} PÓ|ZIEL FERTIG +{0} STAUB|HEDEF TAMAM +{0} TOZ|OBJECTIF FAIT +{0} POUSSIÈRE|目標達成 +{0} 星屑
objective.clean_wave|NO DAMAGE WAVE +ENERGY|无伤通过本波 +能量|ВОЛНА БЕЗ УРОНА +ЭНЕРГИЯ|ONDA SEM DANO +ENERGIA|WELLE OHNE SCHADEN +ENERGIE|HASARSIZ DALGA +ENERJİ|VAGUE SANS DÉGÂTS +ÉNERGIE|無傷WAVE +エネルギー
objective.cast_tactical|Use pilot skill {0} times|使用角色技能 {0} 次|Использовать навык {0} раз|Usar habilidade {0} vezes|Skill {0} Mal nutzen|Yeteneği {0} kez kullan|Utiliser la compétence {0} fois|スキル {0} 回
objective.cast_ultimate|Use ultimate {0} times|使用大招 {0} 次|Использовать ульту {0} раз|Usar ultimate {0} vezes|Ult {0} Mal nutzen|Ultiyi {0} kez kullan|Utiliser l’ultime {0} fois|奥義 {0} 回
objective.route.title|REACH WAVE|推进波次|ДОЙТИ ДО ВОЛНЫ|CHEGAR À ONDA|WELLE ERREICHEN|DALGAYA ULAŞ|ATTEINDRE VAGUE|WAVE到達
objective.route.body|Reach wave {0}.|推进到第 {0} 波。|Дойти до волны {0}.|Chegar à onda {0}.|Welle {0} erreichen.|{0}. dalgaya ulaş.|Atteindre la vague {0}.|Wave {0} まで進む。
objective.clean.title|NO DAMAGE|无伤清波|БЕЗ УРОНА|SEM DANO|OHNE SCHADEN|HASARSIZ|SANS DÉGÂTS|無傷
objective.clean.body|Clear {0} waves without taking damage.|无伤通过 {0} 波。|Пройти {0} волн без урона.|Passar {0} ondas sem dano.|{0} Wellen ohne Schaden schaffen.|Hasarsız {0} dalga geç.|Finir {0} vagues sans dégâts.|無傷で {0} Wave クリア。
objective.tempo.title|COMBO GOAL|连击目标|ЦЕЛЬ КОМБО|META DE COMBO|KOMBO-ZIEL|KOMBO HEDEFİ|OBJECTIF COMBO|コンボ目標
objective.tempo.body|Reach combo X{0} before taking damage.|受伤前达到连击X{0}。|Достичь комбо X{0} до урона.|Chegar ao combo X{0} antes de levar dano.|Vor Schaden Kombo X{0} erreichen.|Hasar almadan X{0} kombo yap.|Atteindre combo X{0} avant les dégâts.|被弾前にコンボX{0}。
objective.pilot.astra.title|CLEAR BULLETS|清除红弹|ЧИСТИТЬ ПУЛИ|LIMPAR TIROS|KUGELN RÄUMEN|MERMI TEMİZLE|NETTOYER TIRS|弾消し
objective.pilot.astra.body|Graze or clear {0} red bullets.|擦弹或清除 {0} 个红弹。|Задеть или очистить {0} красных пуль.|Raspar ou limpar {0} tiros vermelhos.|{0} rote Kugeln streifen oder räumen.|{0} kırmızı mermi sıyır veya temizle.|Frôler ou nettoyer {0} tirs rouges.|赤弾を {0} 回グレイズ/消去。
objective.pilot.vesper.title|USE SKILL|使用技能|ИСП. НАВЫК|USAR HABIL.|SKILL NUTZEN|YETENEK KULLAN|UTILISER COMP.|スキル使用
objective.pilot.vesper.body|Use pilot skill {0} times.|使用角色技能 {0} 次。|Использовать навык {0} раз.|Usar habilidade {0} vezes.|Skill {0} Mal nutzen.|Yeteneği {0} kez kullan.|Utiliser la compétence {0} fois.|スキル {0} 回。
objective.pilot.kairo.title|USE SKILL|使用技能|ИСП. НАВЫК|USAR HABIL.|SKILL NUTZEN|YETENEK KULLAN|UTILISER COMP.|スキル使用
objective.pilot.kairo.body|Use pilot skill {0} times.|使用角色技能 {0} 次。|Использовать навык {0} раз.|Usar habilidade {0} vezes.|Skill {0} Mal nutzen.|Yeteneği {0} kez kullan.|Utiliser la compétence {0} fois.|スキル {0} 回。
objective.pilot.sol.title|DEFEAT ENEMIES|击败敌人|УБИТЬ ВРАГОВ|DERROTAR INIMIGOS|FEINDE BESIEGEN|DÜŞMAN YEN|VAINCRE ENNEMIS|敵撃破
objective.pilot.sol.body|Defeat {0} enemies.|击败 {0} 个敌人。|Победить {0} врагов.|Derrotar {0} inimigos.|{0} Feinde besiegen.|{0} düşman yen.|Vaincre {0} ennemis.|敵を {0} 体撃破。
objective.pilot.nyx.title|CLEAR BULLETS|处理红弹|ЧИСТИТЬ ПУЛИ|LIMPAR TIROS|KUGELN RÄUMEN|MERMI TEMİZLE|NETTOYER TIRS|弾消し
objective.pilot.nyx.body|Graze or clear {0} red bullets.|擦弹或清除 {0} 个红弹。|Задеть или очистить {0} красных пуль.|Raspar ou limpar {0} tiros vermelhos.|{0} rote Kugeln streifen oder räumen.|{0} kırmızı mermi sıyır veya temizle.|Frôler ou nettoyer {0} tirs rouges.|赤弾を {0} 回処理。
objective.pilot.rook.title|USE SKILL|使用技能|ИСП. НАВЫК|USAR HABIL.|SKILL NUTZEN|YETENEK KULLAN|UTILISER COMP.|スキル使用
objective.pilot.rook.body|Use pilot skill {0} times.|使用角色技能 {0} 次。|Использовать навык {0} раз.|Usar habilidade {0} vezes.|Skill {0} Mal nutzen.|Yeteneği {0} kez kullan.|Utiliser la compétence {0} fois.|スキル {0} 回。
objective.pilot.lyra.title|COLLECT EXP|拾取经验|СОБРАТЬ ОПЫТ|COLETAR EXP|EP SAMMELN|EXP TOPLA|RAMASSER EXP|EXP回収
objective.pilot.lyra.body|Collect {0} EXP blocks before they vanish.|在消失前拾取 {0} 个经验块。|Собрать {0} блоков опыта до исчезновения.|Coletar {0} blocos de EXP antes de sumir.|{0} EP-Blöcke sammeln, bevor sie verschwinden.|Kaybolmadan {0} EXP bloğu topla.|Ramasser {0} blocs d’EXP avant disparition.|消える前にEXPブロック {0} 個回収。
objective.pilot.orion.title|DEFEAT BOSSES|击败Boss|УБИТЬ БОССОВ|DERROTAR CHEFES|BOSSE BESIEGEN|BOSS YEN|VAINCRE BOSS|ボス撃破
objective.pilot.orion.body|Defeat {0} bosses.|击败 {0} 个 Boss。|Победить {0} боссов.|Derrotar {0} chefes.|{0} Bosse besiegen.|{0} boss yen.|Vaincre {0} boss.|Bossを {0} 体撃破。
next.boss|NEXT: {0}|下一波：{0}|ДАЛЬШЕ: {0}|PRÓXIMA: {0}|NÄCHSTE: {0}|SIRADA: {0}|SUIVANT : {0}|次：{0}
next.primary|NEXT: {0}  PRIMARY: {1}|下一波：{0}  主敌：{1}|ДАЛЬШЕ: {0}  ЦЕЛЬ: {1}|PRÓXIMA: {0}  ALVO: {1}|NÄCHSTE: {0}  PRIMÄR: {1}|SIRADA: {0}  ANA: {1}|SUIVANT : {0}  CIBLE : {1}|次：{0}  主目標：{1}
next.primary_support|NEXT: {0}  PRIMARY: {1}    SUPPORT: {2}|下一波：{0}  主敌：{1}    辅助：{2}|ДАЛЬШЕ: {0}  ЦЕЛЬ: {1}  ПОДДЕРЖКА: {2}|PRÓXIMA: {0}  ALVO: {1}  APOIO: {2}|NÄCHSTE: {0}  PRIMÄR: {1}  SUPPORT: {2}|SIRADA: {0}  ANA: {1}  DESTEK: {2}|SUIVANT : {0}  CIBLE : {1}  SOUTIEN : {2}|次：{0}  主目標：{1}  支援：{2}
choice.instant|Instant|即时|Мгновенно|Instantâneo|Sofort|Anlık|Instantané|即時
choice.tactic|Tactic|战术|Тактика|Tática|Taktik|Taktik|Tactique|戦術
choice.risk|Risk|风险|Риск|Risco|Risiko|Risk|Risque|リスク
choice.contract|Challenge|挑战|Испытание|Desafio|Herausforderung|Meydan Okuma|Défi|挑戦
choice.meta|Map|星图|Карта|Mapa|Karte|Harita|Carte|星図
choice.capstone|Max Rank|满级|Макс|Máx|Max|Maks|Max|最大
choice.path.weapon|WEAPON|武器|ОРУЖИЕ|ARMA|WAFFE|SİLAH|ARME|武器
choice.path.defense|SURVIVE|生存|ЗАЩИТА|DEFESA|SCHUTZ|SAVUNMA|DÉFENSE|防御
choice.path.skill|SKILL|技能|НАВЫК|HABIL.|SKILL|YETENEK|COMP.|スキル
choice.path.flow|CHAIN|连锁|СВЯЗЬ|CADEIA|KETTE|ZİNCİR|CHAÎNE|連鎖
choice.path.economy|GROWTH|成长|РОСТ|GANHO|WACHSTUM|BÜYÜME|GAIN|成長
choice.badge.pilot|PILOT KIT|角色专属|ПИЛОТ|PILOTO|PILOT|PİLOT|PILOTE|機体専用
choice.badge.synergy|SYNERGY|协同|СИНЕРГИЯ|SINERGIA|SYNERGIE|SİNERJİ|SYNERGIE|シナジー
choice.badge.momentum|MOMENTUM|流派惯性|ИНЕРЦИЯ|IMPULSO|MOMENTUM|İVME|ÉLAN|流れ
choice.badge.capstone|MAX RANK|满级|МАКС|MÁX|MAX|MAKS|MAX|最大
choice.momentum.open|Build direction open|构筑方向未锁定|Направление свободно|Direção aberta|Build offen|Yön açık|Build ouvert|方針自由
choice.momentum.focus|Build leaning: {0}|构筑倾向：{0}|Уклон: {0}|Rumo: {0}|Tendenz: {0}|Yön: {0}|Orientation : {0}|方針：{0}
build.panel.title|BUILD VECTOR|构筑方向|ВЕКТОР БИЛДА|VETOR DE BUILD|BUILD-VEKTOR|YAPI VEKTÖRÜ|VECTEUR BUILD|ビルド方針
build.panel.open|Open draft|自由选择|Свободный выбор|Escolha livre|Freie Wahl|Serbest seçim|Choix libre|自由選択
build.panel.focus|Leaning {0}|偏向 {0}|Уклон {0}|Rumo {0}|Tendenz {0}|Yön {0}|Orientation {0}|方針 {0}
build.breakthrough|{0} Breakthrough {1}|{0}突破 {1}|Прорыв {0} {1}|Ruptura {0} {1}|{0}-Durchbruch {1}|{0} Atılımı {1}|Percée {0} {1}|{0}突破 {1}
build.breakthrough.weapon|Weapon upgrades now hit harder and appear more often.|武器升级伤害更高，并更容易出现。|Улучшения оружия сильнее и появляются чаще.|Upgrades de arma ficam fortes e aparecem mais.|Waffen-Upgrades werden stärker und häufiger.|Silah geliştirmeleri güçlenir ve daha sık gelir.|Les améliorations d’arme frappent plus fort et reviennent plus souvent.|武器強化が強くなり、出やすくなる。
build.breakthrough.defense|Survival upgrades give more hull, damage reduction, and safety time.|生存升级提供更多生命、减伤和安全时间。|Защита дает больше корпуса, снижения урона и безопасного времени.|Defesa dá mais casco, redução de dano e tempo seguro.|Survival gibt mehr Hülle, Schadensminderung und Schutzzeit.|Savunma daha çok gövde, hasar azaltma ve güvenli süre verir.|La survie donne plus de coque, réduction et temps sûr.|生存強化で耐久、軽減、安全時間が増える。
build.breakthrough.skill|Skill upgrades reduce cooldowns and give more energy.|技能升级降低冷却，并提供更多能量。|Навыки режут откаты и дают больше энергии.|Habilidades reduzem recarga e dão energia.|Skill-Upgrades senken Cooldowns und geben Energie.|Yetenek geliştirmesi beklemeyi azaltır ve enerji verir.|Les compétences réduisent les délais et rendent énergie.|スキル強化でCD短縮、エネルギー増。
build.breakthrough.flow|Chain upgrades make hits jump, split, and clear groups better.|连锁升级让命中更容易跳跃、分裂并清群。|Цепи заставляют удары прыгать, делиться и чистить группы.|Cadeia faz acertos saltarem, dividirem e limpar grupos.|Ketten lassen Treffer springen, splittern und Gruppen räumen.|Zincir isabeti sıçratır, böler ve grubu temizler.|La chaîne fait sauter, diviser et nettoyer les groupes.|連鎖強化で命中が跳び、分裂し、群れを処理。
build.breakthrough.economy|Growth upgrades pull more EXP and improve the next wave reward.|成长升级吸取更多经验，并提高下一波奖励。|Рост тянет больше опыта и улучшает награду волны.|Crescimento puxa mais EXP e melhora a próxima recompensa.|Wachstum zieht mehr EP und stärkt die nächste Belohnung.|Büyüme daha çok EXP çeker ve sonraki ödülü artırır.|La croissance attire plus d’EXP et améliore la prochaine récompense.|成長強化でEXP吸引と次波報酬が上がる。
upgrade.title|CHOOSE A BUILD UPGRADE|选择一个升级|ВЫБЕРИ УЗЕЛ СБОРКИ|ESCOLHA UM UPGRADE|BUILD-UPGRADE WÄHLEN|YAPI GELİŞİMİ SEÇ|CHOISIS UNE AMÉLIORATION|強化を選択
upgrade.hint|1 / 2 / 3 or click. Gamepad: LS/D-Pad choose, A select, X reroll.|按 1 / 2 / 3 或点击。手柄：摇杆/方向键选择，A确认，X重抽。|1/2/3 или клик. Геймпад: LS/D-Pad выбор, A принять, X реролл.|1/2/3 ou clique. Controle: LS/D-Pad escolhe, A pega, X reroll.|1/2/3 oder Klick. Pad: LS/D-Pad wählen, A nehmen, X neu.|1/2/3 veya tıkla. Gamepad: LS/D-Pad seç, A al, X yenile.|1/2/3 ou clic. Manette : LS/D-Pad choisir, A prendre, X relancer.|1/2/3またはクリック。パッド：LS/十字で選択、A決定、X再抽選。
upgrade.reroll|REROLL {0}|重抽 {0}|РЕРОЛЛ {0}|REROLL {0}|NEU {0}|YENİLE {0}|RELANCE {0}|再抽選 {0}
upgrade.rank_change|RANK {0}  >  {1}|等级 {0}  >  {1}|РАНГ {0}  >  {1}|RANK {0}  >  {1}|RANG {0}  >  {1}|RÜTBE {0}  >  {1}|RANG {0}  >  {1}|ランク {0}  >  {1}
upgrade.delta.label|THIS LEVEL|本级提升|ЭТОТ РАНГ|ESTE RANK|DIESE STUFE|BU SEVİYE|CE RANG|このランク
upgrade.select_gamepad|A SELECT|A 选择|A ВЫБОР|A ESCOLHER|A WÄHLEN|A SEÇ|A CHOISIR|A 選択
upgrade.select_key|PRESS {0}|按 {0} 选择|ЖМИ {0}|APERTE {0}|TASTE {0}|{0} BAS|TOUCHE {0}|{0} で選択
rank|Rank {0}|等级 {0}|Ранг {0}|Rank {0}|Rang {0}|Rütbe {0}|Rang {0}|ランク {0}
end.victory.title|EXPEDITION COMPLETE|远征完成|ЭКСПЕДИЦИЯ ЗАВЕРШЕНА|EXPEDIÇÃO CONCLUÍDA|EXPEDITION ABGESCHLOSSEN|SEFER TAMAMLANDI|EXPÉDITION TERMINÉE|遠征完了
end.defeat.title|RUN ENDED|远征结束|ЗАБЕГ ЗАВЕРШЕН|RUN ENCERRADA|RUN BEENDET|SEFER BİTTİ|RUN TERMINÉ|遠征終了
end.wave|Wave {0}/{1}|波次 {0}/{1}|Волна {0}/{1}|Onda {0}/{1}|Welle {0}/{1}|Dalga {0}/{1}|Vague {0}/{1}|Wave {0}/{1}
end.victory.body|All bosses defeated. Clear time recorded.|已击败全部 Boss，通关用时已记录。|Все боссы побеждены. Время записано.|Todos os chefes derrotados. Tempo registrado.|Alle Bosse besiegt. Zeit gespeichert.|Tüm bosslar yenildi. Süre kaydedildi.|Tous les boss sont vaincus. Temps enregistré.|全Boss撃破。クリア時間を記録。
end.defeat.body|Keep the Star Dust, upgrade, and launch again.|保留本局星尘，升级后再出发。|Пыль сохранена. Улучшись и лети снова.|Você mantém o pó. Melhore e tente de novo.|Staub bleibt. Verbessere dich und starte neu.|Toz sende kalır. Güçlenip tekrar kalk.|Tu gardes la poussière. Améliore-toi et repars.|星屑は保持。強化して再出撃。
end.summary|RUN SUMMARY|本局结算|ИТОГ ЗАБЕГА|RESUMO DA RUN|RUN-ÜBERSICHT|SEFER ÖZETİ|RÉSUMÉ DU RUN|今回の結果
end.wave_label|Wave|波次|Волна|Onda|Welle|Dalga|Vague|Wave
end.dust_label|Star Dust|星尘|Звездная пыль|Pó Estelar|Sternenstaub|Yıldız Tozu|Poussière|星屑
end.run_bonus_label|Run Bonus|流程奖励|Бонус забега|Bônus da run|Run-Bonus|Sefer bonusu|Bonus de run|進行ボーナス
end.goal_bonus_label|Goal Bonus|目标奖励|Бонус цели|Bônus de meta|Zielbonus|Hedef bonusu|Bonus objectif|目標ボーナス
end.time_label|Clear Time|通关用时|Время победы|Tempo de vitória|Bestzeit|Bitirme süresi|Temps de victoire|クリア時間
end.restart|RESTART|重新开始|ПОВТОР|REINICIAR|NEU STARTEN|TEKRAR BAŞLA|REJOUER|再出撃
end.reward|STAR DUST +{0}|星尘 +{0}|ПЫЛЬ +{0}|PÓ +{0}|STAUB +{0}|TOZ +{0}|POUSSIÈRE +{0}|星屑 +{0}
end.objective_bonus|Goal Bonus +{0}|目标奖励 +{0}|Бонус цели +{0}|Bônus de meta +{0}|Zielbonus +{0}|Hedef bonusu +{0}|Bonus objectif +{0}|目標ボーナス +{0}
end.score_bonus|Run Bonus +{0}|流程奖励 +{0}|Бонус забега +{0}|Bônus da run +{0}|Run-Bonus +{0}|Sefer bonusu +{0}|Bonus de run +{0}|進行ボーナス +{0}
end.clear_time|CLEAR TIME {0}|通关用时 {0}|ВРЕМЯ {0}|TEMPO {0}|ZEIT {0}|SÜRE {0}|TEMPS {0}|クリア時間 {0}
end.clear_record|NEW CLEAR RECORD #{0}  {1}|新的通关记录 第{0}名  {1}|НОВЫЙ РЕКОРД №{0}  {1}|NOVO RECORDE #{0}  {1}|NEUE BESTZEIT #{0}  {1}|YENİ REKOR #{0}  {1}|NOUVEAU RECORD N°{0}  {1}|新記録 #{0}  {1}
end.unlock_pilot|NEW PILOT UNLOCKED: {0}|新角色解锁：{0}|НОВЫЙ ПИЛОТ: {0}|NOVO PILOTO: {0}|NEUER PILOT: {0}|YENİ PİLOT: {0}|NOUVEAU PILOTE : {0}|新パイロット解放：{0}
end.meta_hint|Enter/A restart   B/Esc main menu   Y Fighter Bay|Enter/A 重新开始   B/Esc 主界面   Y 战机升级室|Enter/A повтор   B/Esc меню   Y ангар|Enter/A reinicia   B/Esc menu   Y hangar|Enter/A neu   B/Esc Menü   Y Hangar|Enter/A tekrar   B/Esc menü   Y hangar|Entrée/A rejouer   B/Esc menu   Y hangar|Enter/A 再出撃   B/Esc メニュー   Y 格納庫
meta.title|FIGHTER UPGRADE BAY|战机升级室|АНГАР УЛУЧШЕНИЙ|HANGAR DE UPGRADES|JÄGER-UPGRADEHANGAR|SAVAŞÇI GELİŞTİRME HANGARI|HANGAR D’AMÉLIORATION|戦機アップグレード格納庫
meta.subtitle|Install long-term fighter upgrades.|消耗星尘，改装战机。|Ставь долгие улучшения истребителя.|Instale upgrades de longo prazo.|Baue dauerhafte Jäger-Upgrades ein.|Uzun vadeli savaşçı gelişimleri kur.|Installe des améliorations durables.|長期強化を取り付ける。
meta.dust|Star Dust|星尘|Звездная пыль|Pó Estelar|Sternenstaub|Yıldız Tozu|Poussière stellaire|星屑
meta.wallet|STAR DUST {0}|星尘 {0}|ПЫЛЬ {0}|PÓ {0}|STAUB {0}|TOZ {0}|POUSSIÈRE {0}|星屑 {0}
meta.best|BEST WAVE {0}/40   RUNS {2}|最高波次 {0}/40   出航 {2}|ЛУЧШАЯ {0}/40   ВЫЛЕТЫ {2}|MELHOR {0}/40   RUNS {2}|BESTE {0}/40   RUNS {2}|EN İYİ {0}/40   SEFER {2}|MEILLEURE {0}/40   RUNS {2}|最高 {0}/40   遠征 {2}
leader.title|FASTEST CLEARS|最快通关|ЛУЧШЕЕ ВРЕМЯ|MELHORES TEMPOS|BESTZEITEN|EN İYİ SÜRELER|MEILLEURS TEMPS|最速クリア
leader.global_title|GLOBAL FASTEST CLEARS|全球最快通关|МИРОВОЕ ВРЕМЯ|TEMPOS GLOBAIS|GLOBALE BESTZEITEN|KÜRESEL EN İYİ SÜRELER|MEILLEURS TEMPS MONDIAUX|世界最速クリア
leader.rank|#{0}|第 {0} 名|№{0}|#{0}|#{0}|#{0}|N° {0}|#{0}
leader.no_record|--:--|--:--|--:--|--:--|--:--|--:--|--:--|--:--
leader.loading|Loading global records|正在读取全球榜|Загрузка мирового рейтинга|Carregando ranking global|Globale Zeiten laden|Küresel liste yükleniyor|Chargement du classement mondial|世界ランキング読込中
leader.offline|Steam leaderboard unavailable|Steam 排行榜不可用|Рейтинг Steam недоступен|Ranking Steam indisponível|Steam-Rangliste nicht verfügbar|Steam sıralaması yok|Classement Steam indisponible|Steamランキング利用不可
leader.empty|No global clears yet|暂无全球通关记录|Пока нет мировых зачисток|Sem clears globais ainda|Noch keine globalen Clears|Henüz küresel bitiriş yok|Aucun clear mondial|世界記録なし
goal.title|NEXT TARGET|下个目标|СЛЕДУЮЩАЯ ЦЕЛЬ|PRÓXIMA META|NÄCHSTES ZIEL|SONRAKİ HEDEF|PROCHAIN BUT|次の目標
goal.unlock_pilot|Play {0} once to unlock {1}|用 {0} 完成一局，解锁 {1}|Сыграй за {0} один вылет и открой {1}|Jogue uma vez com {0} para liberar {1}|Spiele einmal mit {0}, um {1} freizuschalten|{1} için {0} ile bir sefer oyna|Joue une fois avec {0} pour débloquer {1}|{0}で1回出撃して{1}を解放
goal.clear_40|Clear wave 40 and finish a run|通关 40 波，完成远征|Пройди 40 волн и заверши вылет|Passe a onda 40 e conclua a run|Schaffe Welle 40 und beende den Run|40. dalgayı geç ve seferi bitir|Passe la vague 40 et termine le run|40波を突破して遠征完了
goal.beat_record|Beat your best clear time {0}|突破最快通关 {0}|Побей лучшее время {0}|Bata seu melhor tempo {0}|Unterbiete deine Bestzeit {0}|En iyi süreni geç {0}|Bats ton meilleur temps {0}|最速記録 {0} を更新
goal.set_record|Finish a full clear to set a time|完成一次通关，留下最快时间|Заверши полный проход и поставь время|Conclua uma vitória para marcar tempo|Beende einen Sieg und setze eine Zeit|Tam zaferle süre kaydet|Termine une victoire pour poser un temps|一度クリアして記録を残す
meta.open_hint|Open the Fighter Bay between runs.|每局结束后可进入战机升级室。|Открывай ангар между вылетами.|Abra o hangar entre runs.|Öffne den Hangar zwischen Runs.|Sefer arasında hangarı aç.|Ouvre le hangar entre deux runs.|出撃の合間に格納庫を開く。
meta.buy_hint|Click a module or press A to install.|点击模块或按 A 安装。|Кликни модуль или нажми A.|Clique no módulo ou aperte A.|Klicke ein Modul oder drücke A.|Modüle tıkla veya A bas.|Clique un module ou appuie sur A.|モジュールをクリック、またはAで装着。
meta.back|BACK|返回|НАЗАД|VOLTAR|ZURÜCK|GERİ|RETOUR|戻る
meta.cost|COST {0}|花费 {0}|ЦЕНА {0}|CUSTO {0}|KOSTEN {0}|BEDEL {0}|COÛT {0}|コスト {0}
meta.max|MAX|已满|МАКС|MÁX|MAX|MAKS|MAX|最大
meta.rank|Rank {0}/{1}|等级 {0}/{1}|Ранг {0}/{1}|Rank {0}/{1}|Rang {0}/{1}|Rütbe {0}/{1}|Rang {0}/{1}|ランク {0}/{1}
meta.bought|UPGRADED|升级完成|УЛУЧШЕНО|APRIMORADO|VERSTÄRKT|GELİŞTİ|AMÉLIORÉ|強化完了
meta.short|NEED {0} MORE|还差 {0}|НУЖНО {0}|FALTA {0}|NOCH {0}|{0} EKSİK|MANQUE {0}|あと {0}
settings.title|SETTINGS|设置|НАСТРОЙКИ|CONFIGURAÇÕES|EINSTELLUNGEN|AYARLAR|PARAMÈTRES|設定
settings.subtitle|Tune audio, language, resolution, visual quality, and guide access.|调整音量、语言、分辨率、画面品质，也可查看说明。|Настрой звук, язык, разрешение, качество и справочник.|Ajuste áudio, idioma, resolução, qualidade e guia.|Audio, Sprache, Auflösung, Grafik und Guide anpassen.|Ses, dil, çözünürlük, kalite ve rehberi ayarla.|Règle audio, langue, résolution, qualité et guide.|音量、言語、解像度、画質、ガイドを調整。
settings.guide|GAME GUIDE|游戏说明|СПРАВОЧНИК|GUIA DO JOGO|SPIELGUIDE|OYUN REHBERİ|GUIDE DE JEU|ゲームガイド
settings.main_menu|MAIN MENU|回到主界面|ГЛАВНОЕ МЕНЮ|MENU PRINCIPAL|HAUPTMENÜ|ANA MENÜ|MENU PRINCIPAL|メインメニュー
settings.delete_save|DELETE SAVE|删除存档|УДАЛИТЬ СЕЙВ|APAGAR SAVE|SPIELSTAND LÖSCHEN|KAYDI SİL|SUPPRIMER SAUVEGARDE|セーブ削除
settings.delete_confirm|CLICK AGAIN TO DELETE|再次点击确认删除|ЕЩЕ РАЗ ДЛЯ УДАЛЕНИЯ|CLIQUE DE NOVO|NOCHMAL KLICKEN|SİLMEK İÇİN TEKRAR|RECLIQUER POUR SUPPRIMER|もう一度で削除
settings.delete_warning|Clears progress, unlocks, Fighter Bay upgrades, and records. Keeps local settings.|清空进度、解锁、战机升级室改装和记录。保留本机设置。|Стирает прогресс, пилотов, ангар и рекорды. Настройки останутся.|Apaga progresso, pilotos, hangar e recordes. Mantém ajustes locais.|Löscht Fortschritt, Piloten, Hangar-Upgrades und Rekorde. Lokale Einstellungen bleiben.|İlerleme, pilotlar, hangar ve kayıtları siler. Yerel ayarlar kalır.|Efface progression, pilotes, hangar et records. Garde les réglages locaux.|進行、機体、格納庫強化、記録を消去。ローカル設定は残る。
settings.delete_notice|SAVE DATA CLEARED|存档已删除|СЕЙВ УДАЛЕН|SAVE APAGADO|SPIELSTAND GELÖSCHT|KAYIT SİLİNDİ|SAUVEGARDE EFFACÉE|セーブ削除済み
settings.resume|RESUME|继续游戏|ПРОДОЛЖИТЬ|CONTINUAR|FORTSETZEN|DEVAM|REPRENDRE|再開
settings.back|BACK|返回|НАЗАД|VOLTAR|ZURÜCK|GERİ|RETOUR|戻る
settings.music|Music Volume|音乐音量|Музыка|Volume da música|Musiklautstärke|Müzik sesi|Volume musique|音楽音量
settings.sfx|SFX Volume|音效音量|Эффекты|Volume dos efeitos|Effektlautstärke|Efekt sesi|Volume effets|効果音音量
settings.language|Language|语言|Язык|Idioma|Sprache|Dil|Langue|言語
settings.resolution|Resolution|分辨率|Разрешение|Resolução|Auflösung|Çözünürlük|Résolution|解像度
settings.quality|Visual Quality|画面品质|Качество|Qualidade visual|Grafikqualität|Görsel kalite|Qualité visuelle|画質
settings.quality.low|Low|低|Низкое|Baixa|Niedrig|Düşük|Basse|低
settings.quality.medium|Medium|中|Среднее|Média|Mittel|Orta|Moyenne|中
settings.quality.high|High|高|Высокое|Alta|Hoch|Yüksek|Élevée|高
settings.quality.ultra|Ultra|极致|Ультра|Ultra|Ultra|Ultra|Ultra|最高
settings.adjust_hint|Left / Right to adjust. Enter or A confirms.|左右调整，Enter 或 A 确认。|Влево/вправо - изменить. Enter или A - подтвердить.|Esquerda/direita ajusta. Enter ou A confirma.|Links/rechts ändern. Enter oder A bestätigt.|Sol/sağ ayarla. Enter veya A onaylar.|Gauche/droite ajuste. Entrée ou A valide.|左右で調整。Enter または A で決定。
guide.title|GAME GUIDE|游戏说明|СПРАВОЧНИК|GUIA DO JOGO|SPIELGUIDE|OYUN REHBERİ|GUIDE DE JEU|ゲームガイド
guide.subtitle|Controls, EXP, combo, upgrades, and progress rules are listed here.|这里列出操作、经验、连击、升级和进程规则。|Здесь правила управления, опыта, комбо, улучшений и прогресса.|Controles, EXP, combo, upgrades e progresso ficam aqui.|Hier stehen Steuerung, EP, Kombo, Upgrades und Fortschritt.|Kontrol, EXP, kombo, geliştirme ve ilerleme burada.|Commandes, EXP, combo, améliorations et progression sont ici.|操作、EXP、コンボ、強化、進行ルールを確認。
guide.page_hint|Left or Right switches tabs. Back returns to settings.|左右切换页签。返回回到设置。|Влево или вправо меняет вкладку. Назад к настройкам.|Esquerda ou direita troca abas. Voltar abre ajustes.|Links oder rechts wechselt Tabs. Zurück öffnet Einstellungen.|Sol veya sağ sekme değiştirir. Geri ayarlara döner.|Gauche ou droite change d’onglet. Retour ouvre les paramètres.|左右でタブ切替。戻るで設定へ。
guide.tab.0|BASICS|基础|ОСНОВЫ|BÁSICO|BASIS|TEMEL|BASES|基本
guide.tab.1|COMBO|连击|КОМБО|COMBO|KOMBO|KOMBO|COMBO|コンボ
guide.tab.2|BUILDS|构筑|СБОРКИ|BUILDS|BUILDS|YAPILAR|BUILDS|ビルド
guide.tab.3|PROGRESS|进程|ПРОГРЕСС|PROGRESSO|FORTSCHRITT|İLERLEME|PROGRESSION|進行
guide.basic.0|Move with WASD, arrows, or left stick. Mouse/right stick aims dash; weapons auto-lock and fire.|WASD、方向键或左摇杆移动。鼠标/右摇杆决定冲刺方向，武器自动锁敌开火。|WASD, стрелки или левый стик для движения. Мышь/правый стик задают рывок; оружие автостреляет.|Mova com WASD, setas ou analógico esquerdo. Mouse/stick direito mira o dash; armas travam e atiram.|WASD, Pfeile oder linker Stick bewegen. Maus/rechter Stick zielt den Sprint; Waffen feuern automatisch.|WASD, yön tuşları veya sol çubuk hareket. Mouse/sağ çubuk atılma yönünü verir; silahlar oto ateş eder.|WASD, flèches ou stick gauche pour bouger. Souris/stick droit vise le dash; armes auto-lock et tirent.|WASD、矢印、左スティックで移動。マウス/右スティックでダッシュ方向、武器は自動ロック射撃。
guide.basic.1|Enemy bullets are always red and dangerous. Gray blocks are EXP pickups and disappear if ignored.|敌方子弹永远是红色并且危险。灰色方块是经验，太久不吃会消失。|Вражеские пули всегда красные и опасные. Серые блоки это опыт, он исчезает.|Tiros inimigos são sempre vermelhos e perigosos. Blocos cinza são EXP e somem.|Feindkugeln sind immer rot und gefährlich. Graue Blöcke sind EP und verschwinden.|Düşman mermileri kırmızı ve tehlikeli. Gri bloklar TP, beklerse kaybolur.|Les tirs ennemis sont rouges et dangereux. Les blocs gris sont de l’EXP et expirent.|敵弾は常に赤く危険。灰色ブロックはEXPで放置すると消える。
guide.basic.2|SPACE or X/RB uses the pilot skill. Skills can pierce, clear bullets, heal, pull enemies, block shots, or mark targets.|空格或 X/RB 使用角色技能。技能可能是穿透、清弹、回血、牵引、挡弹或标记。|Пробел или X/RB использует навык. Он может пробивать, чистить пули, лечить, тянуть, блокировать или метить.|Espaço ou X/RB usa habilidade. Ela pode perfurar, limpar, curar, puxar, bloquear ou marcar.|Leertaste oder X/RB nutzt den Skill. Er kann durchbohren, räumen, heilen, ziehen, blocken oder markieren.|Space veya X/RB yetenek kullanır. Delme, temizleme, iyileştirme, çekme, bloklama veya işaretleme yapabilir.|Espace ou X/RB utilise la compétence. Elle peut percer, nettoyer, soigner, attirer, bloquer ou marquer.|Space/X/RBでスキル。貫通、弾消し、回復、引き寄せ、防御、標的付けがある。
guide.basic.3|F/E or Y/RT spends high energy to clear nearby bullets. It has a cooldown, so save it for crowded moments.|F/E 或 Y/RT 消耗大量能量清除近身子弹。它有冷却，适合留给最乱的时候。|F/E или Y/RT тратит много энергии и чистит пули рядом. Есть откат, береги для хаоса.|F/E ou Y/RT gasta muita energia para limpar tiros perto. Tem recarga, guarde para caos.|F/E oder Y/RT räumt nahe Kugeln für viel Energie. Es hat Abklingzeit, spare es für Druck.|F/E veya Y/RT çok enerjiyle yakın mermi temizler. Bekleme var, kalabalığa sakla.|F/E ou Y/RT dépense beaucoup d’énergie pour nettoyer près de toi. Garde-le pour les moments denses.|F/EまたはY/RTで高エネルギー消費の近距離弾消し。CDがあるので温存。
guide.combo.0|Combo stays until you take damage. Kills, bullet clears, and EXP pickups add combo.|受伤前连击会一直保留。击杀、清弹和拾取经验都会增加连击。|Комбо держится до урона. Убийства, очистка пуль и опыт добавляют комбо.|Combo dura até levar dano. Abates, limpezas e EXP aumentam combo.|Kombo bleibt bis du Schaden nimmst. Kills, Räumen und EP erhöhen Kombo.|Hasar alana kadar kombo sürer. Öldürme, temizleme ve EXP kombo ekler.|Le combo reste jusqu’aux dégâts. Kills, nettoyage et EXP l’augmentent.|被弾までコンボ維持。撃破、弾消し、EXP回収で増える。
guide.combo.1|Higher combo shortens enemy spawn cooldown a little. Later waves also spawn faster by default.|连击越高，刷怪冷却会略微缩短。越到后期，基础刷怪也会更快。|Высокое комбо немного режет задержку спавна. Поздние волны сами быстрее.|Combo alto reduz um pouco o tempo de spawn. Ondas tardias já nascem mais rápido.|Höhere Kombo senkt Spawn-Cooldown etwas. Späte Wellen spawnen von selbst schneller.|Yüksek kombo doğma beklemesini az azaltır. Geç dalgalar zaten hızlı doğar.|Un combo élevé réduit un peu le délai d’apparition. Les vagues tardives accélèrent aussi.|高コンボで出現CDが少し短縮。後半Waveは基本出現も速い。
guide.combo.2|Taking damage breaks combo and removes the combo spawn bonus.|受伤会打断连击，并移除连击带来的刷怪加速。|Урон сбивает комбо и убирает бонус спавна.|Levar dano quebra combo e remove bônus de spawn.|Schaden bricht Kombo und entfernt den Spawn-Bonus.|Hasar kombo kırar ve doğma bonusunu siler.|Les dégâts cassent le combo et retirent le bonus d’apparition.|被弾でコンボが切れ、出現加速も消える。
guide.combo.3|Play safe to survive, or keep combo high to finish faster and face more enemies sooner.|求稳可以慢慢打；保持高连击可以更快通关，但敌人会更早变多。|Играй безопасно ради выживания или держи комбо, чтобы идти быстрее и раньше встретить толпы.|Jogue seguro para viver ou mantenha combo para terminar rápido e ver mais inimigos cedo.|Sicher spielen zum Überleben oder hohe Kombo für schnelleres Ende und frühere Gegnerwellen.|Güvenli oyna veya yüksek kombo tutup hızlı bitir ama düşman erken çoğalır.|Joue sûr pour survivre ou garde le combo pour finir plus vite avec plus d’ennemis.|安全に生きるか、高コンボで早く進み敵増加を早めるか。
guide.build.0|Enemies drop gray EXP blocks. A full EXP bar pauses the fight and opens upgrade choices.|敌人掉落灰色经验块。经验条满后暂停战斗并弹出升级选择。|Враги роняют серый опыт. Полная шкала ставит бой на паузу и открывает выбор.|Inimigos soltam EXP cinza. Barra cheia pausa e abre upgrades.|Feinde lassen graue EP fallen. Volle Leiste pausiert und öffnet Upgrades.|Düşmanlar gri TP düşürür. Bar dolunca savaş durur ve seçim açılır.|Les ennemis lâchent de l’EXP grise. Barre pleine met en pause et ouvre un choix.|敵は灰色EXPを落とす。満タンで戦闘停止し強化選択。
guide.build.1|Pilot cards and public cards appear together. Repeated choices make similar cards more likely later.|角色专属卡和公共卡会一起出现。连续选择同类效果，后面更容易刷到同类卡。|Карты пилота и общие карты смешаны. Повторный выбор повышает шанс похожих карт.|Cartas do piloto e públicas aparecem juntas. Escolhas repetidas puxam cartas parecidas.|Piloten- und allgemeine Karten mischen sich. Wiederholte Wahl erhöht ähnliche Karten.|Pilot ve ortak kartlar birlikte gelir. Benzer seçimler ileride benzer kart getirir.|Cartes pilote et communes se mélangent. Choisir pareil augmente leur chance.|固有カードと共通カードが混ざる。同系統を選ぶほど出やすい。
guide.build.2|At max rank, some upgrades gain a max-rank effect that changes the build: chain, split, drones, shield, rhythm, or precision.|部分升级满级后会获得满级效果，改变流派：连锁、分裂、无人机、护盾、节拍或精准。|На максимальном ранге часть улучшений меняет билд: цепь, раскол, дроны, щит, ритм или точность.|No nível máximo, alguns upgrades mudam a build: cadeia, divisão, drones, escudo, ritmo ou precisão.|Auf Max-Rang ändern manche Upgrades den Build: Kette, Splitter, Drohnen, Schild, Rhythmus oder Präzision.|Maks seviyede bazı geliştirmeler yapıyı değiştirir: zincir, bölünme, dron, kalkan, ritim veya keskinlik.|Au rang max, certains effets changent le build : chaîne, éclats, drones, bouclier, rythme ou précision.|最大ランクで一部強化が変化。連鎖、分裂、ドローン、盾、リズム、精密。
guide.build.3|Rerolls are limited. Use them when all choices do not fit your pilot or current build.|重抽次数有限。三个选项都不适合角色或当前流派时再用。|Рероллы ограничены. Используй, если все три карты не подходят пилоту или билду.|Rerolls são limitados. Use quando as três cartas não servem ao piloto ou build.|Neuwürfe sind begrenzt. Nutze sie, wenn alle Karten nicht zu Pilot oder Build passen.|Yenileme sınırlı. Üç seçim de pilota veya yapıya uymuyorsa kullan.|Relances limitées. Utilise-les si les trois cartes ne collent pas au pilote ou au build.|再抽選は有限。3枚とも機体やビルドに合わない時に使う。
guide.progress.0|Pilots unlock in order. Play one run with the current pilot to unlock the next pilot.|角色按顺序解锁。用当前角色玩一局，就能解锁下一个角色。|Пилоты открываются по порядку. Сыграй один вылет текущим пилотом, чтобы открыть следующего.|Pilotos abrem em ordem. Jogue uma run com o atual para abrir o próximo.|Piloten öffnen der Reihe nach. Ein Run mit aktuellem Pilot öffnet den nächsten.|Pilotlar sırayla açılır. Mevcut pilotla bir sefer oyna, sonraki açılır.|Les pilotes se débloquent dans l’ordre. Une run avec l’actuel ouvre le suivant.|機体は順番に解放。現在の機体で1回遊ぶと次が開く。
guide.progress.1|Star Dust upgrades your fighter in the Fighter Bay. Costs are tuned as a long journey, not something to finish in one run.|星尘用于在战机升级室改装战机。价格按长线成长设计，不应该一局买满。|Пыль улучшает истребитель в ангаре. Цены рассчитаны на долгий путь.|Pó Estelar melhora o caça no hangar. Custos são jornada longa.|Sternenstaub verbessert den Jäger im Hangar. Kosten sind Langzeitreise.|Yıldız Tozu hangarda savaşçıyı geliştirir. Maliyetler uzun yol içindir.|La poussière améliore le chasseur au hangar. Les coûts visent le long terme.|星屑で格納庫の戦機を強化。価格は長期進行向け。
guide.progress.2|Enemy waves and bosses are partly random. Runs will not have the exact same order.|敌人波次和 Boss 有一定随机性，每局顺序不会完全一样。|Волны и боссы частично случайны. Порядок в забегах меняется.|Ondas e chefes são parcialmente aleatórios. A ordem muda entre runs.|Wellen und Bosse sind teils zufällig. Läufe haben nicht dieselbe Reihenfolge.|Dalgalar ve bosslar kısmen rastgele. Sıra her koşuda değişir.|Vagues et boss sont en partie aléatoires. L’ordre change entre runs.|敵Waveとボスは一部ランダム。毎回同じ順番ではない。
guide.progress.3|Main goal: clear 40 waves. Advanced goal: keep combo high and finish faster without losing control.|主要目标是通过 40 波。进阶目标是保持高连击，在不失控的情况下更快通关。|Главная цель: 40 волн. Продвинутая: держать комбо и пройти быстрее без хаоса.|Meta principal: 40 ondas. Meta avançada: combo alto e final rápido sem perder controle.|Hauptziel: 40 Wellen. Fortgeschritten: hohe Kombo und schneller Abschluss ohne Kontrollverlust.|Ana hedef 40 dalga. İleri hedef yüksek kombo ve kontrolü kaybetmeden hızlı bitiş.|But principal: 40 vagues. But avancé: combo élevé et fin rapide sans perdre le contrôle.|目標は40波突破。上級目標は高コンボで制御しながら高速クリア。
guide.0|Move with WASD, arrows, or left stick. Mouse/right stick aims dash; weapons auto-lock and fire.|WASD / 方向键 / 左摇杆移动。鼠标/右摇杆决定冲刺方向，武器自动锁敌开火。|WASD, стрелки или левый стик для движения. Мышь/правый стик задают рывок; оружие автостреляет.|Mova com WASD, setas ou analógico esquerdo. Mouse/stick direito mira o dash; armas travam e atiram.|WASD, Pfeile oder linker Stick bewegen. Maus/rechter Stick zielt den Sprint; Waffen feuern automatisch.|WASD, yön tuşları veya sol çubuk hareket. Mouse/sağ çubuk atılma yönünü verir; silahlar oto ateş eder.|WASD, flèches ou stick gauche. Souris/stick droit vise le dash; armes auto-lock et tirent.|WASD/矢印/左スティックで移動。マウス/右スティックでダッシュ方向、武器は自動ロック射撃。
guide.1|Enemy bullets are always red, and red fire is always danger. Grazing, clearing, and steady hits build focus.|敌方子弹统一为红色，红色永远危险。擦弹、清弹和稳定命中会积累专注。|Вражеские пули всегда красные и опасны. Задевание, очистка и стабильные попадания копят фокус.|Tiros inimigos são sempre vermelhos e perigosos. Raspar, limpar e acertar gera foco.|Feindkugeln sind immer rot und gefährlich. Streifen, Räumen und Treffer bauen Fokus auf.|Düşman mermileri kırmızı ve tehlikelidir. Sıyırma, temizleme ve isabet odak verir.|Les tirs ennemis sont rouges et dangereux. Frôler, nettoyer et toucher charge le focus.|敵弾は赤で常に危険。グレイズ、弾消し、命中で集中が増える。
guide.2|SPACE or X/RB uses your pilot skill. Read the pilot card to see what that skill does.|空格或 X/RB 使用角色技能。技能效果可以看角色卡说明。|Пробел или X/RB использует навык. Его эффект указан на карте пилота.|Espaço ou X/RB usa habilidade. Leia a carta do piloto para ver o efeito.|Leertaste oder X/RB nutzt den Skill. Die Pilotenkarte erklärt den Effekt.|Space veya X/RB yetenek kullanır. Etkisini pilot kartında görürsün.|Espace ou X/RB utilise la compétence. La carte pilote indique son effet.|Space/X/RBでスキル。効果は機体カードに表示。
guide.3|Enemies charge before shooting. When they overheat after shooting, they take bonus damage.|敌人射击前会蓄力。射击后短暂过热，过热时受到额外伤害。|Враги заряжаются перед выстрелом. После залпа перегреваются и получают больше урона.|Inimigos carregam antes de atirar. Depois superaquecem e sofrem mais dano.|Feinde laden vor dem Schuss. Nach dem Feuer überhitzen sie und nehmen mehr Schaden.|Düşman ateşten önce yüklenir. Sonra aşırı ısınır ve fazla hasar alır.|Les ennemis chargent avant de tirer. Après le tir, ils surchauffent et subissent plus de dégâts.|敵は発射前に溜める。発射後に過熱し、追加ダメージを受ける。
guide.4|Right mouse, Shift, A, LB, or LT dashes. Dash briefly grants invulnerability and clears nearby bullets.|右键 / Shift / A / LB / LT 冲刺。冲刺短暂无敌并清除近身子弹。|ПКМ, Shift, A, LB или LT — рывок. Он дает неуязвимость и чистит пули рядом.|Botão direito, Shift, A, LB ou LT dá dash. Dash concede invulnerabilidade curta e limpa tiros.|Rechtsklick, Shift, A, LB oder LT sprintet. Sprint macht kurz unverwundbar und räumt Kugeln.|Sağ tık, Shift, A, LB veya LT atıl. Kısa dokunulmazlık ve yakın mermi temizler.|Clic droit, Shift, A, LB ou LT dash. Brève invulnérabilité et nettoyage proche.|右クリック/Shift/A/LB/LTでダッシュ。短時間無敵と近距離弾消し。
guide.5|F/E or Y/RT spends high energy on an emergency bullet clear. It also has a cooldown.|F / E 或 Y/RT 消耗大量能量释放紧急清弹，并且有冷却。|F/E или Y/RT тратит много энергии на очистку пуль. Есть откат.|F/E ou Y/RT gasta muita energia numa limpeza de emergência. Tem recarga.|F/E oder Y/RT nutzt viel Energie für Noträumung. Es hat Abklingzeit.|F/E veya Y/RT çok enerjiyle acil temizlik yapar. Bekleme var.|F/E ou Y/RT dépense beaucoup d’énergie pour un nettoyage urgent. Recharge incluse.|F/EまたはY/RTで高コストの緊急弾消し。CDあり。
guide.6|Runs include normal, elite, supply, pressure, and boss waves. The order can change each run.|每局包含普通、精英、补给、压迫和 Boss 波。每局顺序可能变化。|В забеге есть обычные, элитные, снабжение, давление и боссы. Порядок меняется.|Runs têm ondas normais, elite, suprimento, pressão e chefe. A ordem pode mudar.|Läufe haben normale, Elite-, Vorrats-, Druck- und Bosswellen. Die Reihenfolge kann wechseln.|Koşularda normal, elit, ikmal, baskı ve boss dalgaları olur. Sıra değişebilir.|Les runs ont vagues normales, élites, ravitaillement, pression et boss. L’ordre varie.|通常、精鋭、補給、圧力、ボスWaveがあり、順番は変わる。
guide.7|Chain builds are good against groups. Split builds are good when you kill enemies quickly.|连锁流适合打密集敌群。分裂流适合快速击杀后滚雪球。|Цепи хороши против групп. Раскол силен, когда быстро убиваешь врагов.|Cadeia é boa contra grupos. Divisão cresce quando você mata rápido.|Ketten sind gut gegen Gruppen. Splitter sind gut bei schnellen Kills.|Zincir kalabalığa iyi gelir. Bölünme hızlı öldürmede büyür.|La chaîne est forte contre les groupes. Les éclats aiment les kills rapides.|連鎖は群れ向け。分裂は素早い撃破で伸びる。
guide.8|Enemies drop gray EXP blocks. They expire if ignored, and a full EXP bar immediately opens a three-card upgrade.|敌人掉落灰色经验块。太久不拾取会消失，经验条满后立刻弹出三选一。|Враги роняют серый опыт. Он исчезает, если медлить. Полная шкала открывает три карты.|Inimigos soltam EXP cinza. Expira se ignorado; barra cheia abre três cartas.|Feinde lassen graue EP fallen. Sie verfallen; volle EP-Leiste öffnet drei Karten.|Düşmanlar gri TP düşürür. Toplanmazsa yok olur; dolu bar üç kart açar.|Les ennemis lâchent de l’EXP grise. Elle expire; barre pleine ouvre trois cartes.|敵は灰色EXPを落とす。放置で消え、満タンで三択強化。
tutorial.1|Red bullets are dangerous. Clear bullets or graze them to gain energy.|红弹很危险。清弹或擦弹可以获得能量。|Красные пули опасны. Чисть или задевай их, чтобы получить энергию.|Tiros vermelhos são perigosos. Limpe ou raspe para ganhar energia.|Rote Kugeln sind gefährlich. Räumen oder streifen gibt Energie.|Kırmızı mermiler tehlikeli. Temizle veya sıyır, enerji kazan.|Les tirs rouges sont dangereux. Nettoie ou frôle pour gagner de l’énergie.|赤弾は危険。消すかグレイズするとエネルギー獲得。
tutorial.2|Press SPACE or X/RB for your pilot skill. Each pilot skill has a different use.|空格或 X/RB 使用角色技能。每个角色技能用途不同。|Пробел или X/RB использует навык. У каждого пилота он разный.|Espaço ou X/RB usa habilidade. Cada piloto usa diferente.|Leertaste oder X/RB nutzt den Skill. Jeder Pilot nutzt ihn anders.|Space veya X/RB yetenek kullanır. Her pilotta kullanımı farklıdır.|Espace ou X/RB utilise la compétence. Chaque pilote a un usage différent.|Space/X/RBでスキル。機体ごとに用途が違う。
tutorial.3|Each pilot has a weapon, pilot skill, and upgrade archetype.|每个角色都有自己的武器、技能和构筑流派。|У каждого пилота свое оружие, навык и архетип.|Cada piloto tem arma, habilidade e arquétipo.|Jeder Pilot hat Waffe, Skill und Buildtyp.|Her pilotun silahı, yeteneği ve yolu var.|Chaque pilote a arme, compétence et archétype.|各機に武器、スキル、強化型がある。
tutorial.9|Beams are dangerous too. Dash or emergency clear to open lanes.|光束同样危险。用冲刺或紧急清弹打开通路。|Лучи тоже опасны. Рывок или очистка откроют путь.|Feixes também ferem. Use dash ou limpeza para abrir caminho.|Strahlen sind gefährlich. Sprint oder Noträumung öffnet Wege.|Işınlar da tehlikeli. Atıl veya temizleme yol açar.|Les rayons sont dangereux. Dash ou nettoyage urgent ouvre une voie.|ビームも危険。ダッシュや緊急弾消しで道を開く。
meta.hull.title|Hull Plating|船体加固|Обшивка корпуса|Blindagem do Casco|Hüllenpanzerung|Gövde Zırhı|Blindage de coque|船体装甲
meta.energy.title|Reactor Seed|能量核心|Зерно реактора|Semente do Reator|Reaktorkeim|Reaktör Çekirdeği|Germe de réacteur|炉心種
meta.weapon.title|Focus Lens|聚焦透镜|Фокусная линза|Lente de Foco|Fokuslinse|Odak Merceği|Lentille de focus|集束レンズ
meta.engine.title|Drift Engine|推进引擎|Дрейф-двигатель|Motor de Deriva|Driftmotor|Sürüklenme Motoru|Moteur de dérive|漂流エンジン
meta.salvage.title|Salvage Rig|回收装置|Сборщик лома|Módulo de Salvagem|Bergungsrig|Hurda Düzeneği|Module de récupération|回収装置
meta.chart.title|Starter Chart|开局星图|Стартовая карта|Mapa Inicial|Startkarte|Başlangıç Haritası|Carte de départ|初期星図
meta.repair.title|Repair Protocol|维修协议|Протокол ремонта|Protocolo de Reparo|Reparaturprotokoll|Onarım Protokolü|Protocole de réparation|修理プロトコル
meta.aegis.title|Aegis Matrix|护盾矩阵|Матрица Эгиды|Matriz Égide|Aegis-Matrix|Kalkan Matrisi|Matrice Égide|イージス行列
meta.nova.title|Emergency Catalyst|清弹催化|Катализатор очистки|Catalisador de Limpeza|Notkatalysator|Acil Katalizör|Catalyseur d’urgence|緊急触媒
meta.drone.title|Drone Dock|无人机坞|Док дронов|Doca Drone|Drohnendock|Dron Yuvası|Dock drone|ドローンドック
meta.tuner.title|Tactical Console|战术控制台|Тактическая консоль|Console Tático|Taktikkonsole|Taktik Konsolu|Console tactique|戦術端末
meta.survey.title|Deep Survey|深空测绘|Глубокая разведка|Sondagem Profunda|Tiefenscan|Derin Tarama|Prospection profonde|深宇宙測量
meta.module.title|Starter Module|开局模块|Стартовый модуль|Módulo Inicial|Startmodul|Başlangıç Modülü|Module de départ|開始モジュール
meta.codex.title|Pilot Blueprint|专属蓝图|Чертеж пилота|Projeto do Piloto|Pilotenplan|Pilot Planı|Plan du pilote|パイロット設計図
meta.combo.title|Combo Engine|连击引擎|Двигатель комбо|Motor de Combo|Combo-Motor|Kombo Motoru|Moteur de combo|コンボエンジン
meta.elite.title|Elite Analysis|精英解析|Анализ элиты|Análise Elite|Eliteanalyse|Elit Analizi|Analyse élite|エリート解析
meta.hull.body|Start every run with more maximum hull.|每局开局拥有更高生命上限。|Каждый вылет начинается с большим корпусом.|Comece cada run com mais casco máximo.|Starte jeden Run mit mehr Hülle.|Her sefere daha yüksek gövdeyle başla.|Commence chaque run avec plus de coque.|各遠征の最大耐久が増える。
meta.energy.body|Start with more energy and a larger energy cap.|提高开局能量和能量上限。|Больше стартовой энергии и лимита.|Mais energia inicial e limite maior.|Mehr Startenergie und höheres Limit.|Daha fazla başlangıç enerjisi ve kapasite.|Plus d’énergie de départ et de réserve.|開始エネルギーと上限が増える。
meta.weapon.body|Weapon damage starts higher before any draft choices.|每局开局武器伤害更高。|Оружие сильнее еще до выбора сборки.|Armas começam causando mais dano.|Waffen starten mit mehr Schaden.|Silah hasarı baştan yükselir.|Les armes commencent plus fortes.|武器初期ダメージが上がる。
meta.engine.body|Move faster and dash harder from wave one.|从第一波开始移动更快、冲刺更强。|С первой волны быстрее движение и рывок.|Mova mais rápido e dê dash melhor desde a onda um.|Ab Welle eins schneller und härter sprinten.|İlk dalgadan itibaren daha hızlı ve sert atıl.|Plus rapide dès la première vague.|Wave1から移動とダッシュ強化。
meta.salvage.body|Earn more Star Dust and pull pickups from farther away.|星尘收益更高，拾取范围更远。|Больше пыли и дальний сбор.|Mais Pó Estelar e atração maior.|Mehr Staub und größere Sammelreichweite.|Daha çok Toz ve daha uzak toplama.|Plus de poussière et aimant plus large.|星屑獲得と吸引範囲が増える。
meta.chart.body|Gain extra rerolls on every upgrade screen.|每次升级选择获得更多重抽。|Больше рероллов на каждом выборе.|Mais rerolls em cada escolha.|Mehr Neuwürfe bei jeder Wahl.|Her seçimde ekstra yenileme.|Relances supplémentaires à chaque choix.|各強化画面の再抽選が増える。
meta.repair.body|Repair drops are slightly more common, and calm moments restore a little hull.|修复掉落小幅提高，脱战后少量回血。|Ремонт падает чуть чаще, паузы немного чинят корпус.|Reparos ficam um pouco mais comuns; calma cura pouco.|Reparaturen fallen etwas öfter, Ruhe heilt wenig Hülle.|Onarım az daha sık düşer, sakin anlar az iyileştirir.|Réparations un peu plus fréquentes, le calme soigne peu.|修理ドロップ少し増加、非戦闘時に少し回復。
meta.aegis.body|Incoming damage is reduced before temporary shields or repairs.|先降低受到的伤害，再计算护盾与修复。|Урон снижается до щитов и ремонта.|Reduz dano antes de escudos e reparos.|Schaden sinkt vor Schild und Reparatur.|Kalkan/onarım öncesi hasarı azaltır.|Réduit les dégâts avant boucliers et soins.|被ダメージを先に軽減。
meta.nova.body|Emergency clears cost less energy and each run starts with more charge.|紧急清弹耗能降低，开局拥有更多能量。|Очистка дешевле, стартовый заряд выше.|Limpezas custam menos e começam mais cheias.|Noträumung kostet weniger und startet geladener.|Acil temizlik ucuzlar, başlangıç enerjisi artar.|Nettoyage urgent moins cher et départ mieux chargé.|緊急弾消しコスト減、開始チャージ増。
meta.drone.body|Begin runs with support drones. Kairo turns this into a stronger swarm.|开局获得支援无人机。环序会把它强化成蜂群。|Стартовые дроны поддержки. Кайро усиливает рой.|Comece com drones; Kairo vira enxame forte.|Starte mit Drohnen; Kairo macht daraus Schwarm.|Destek dronlarıyla başla; Kairo sürüye çevirir.|Drones de départ; Kairo les transforme en essaim.|支援ドローン開始。カイロは群れへ強化。
meta.tuner.body|Pilot skills cool down faster and focus gains return more energy.|角色技能冷却更快，专注收益带来更多能量。|Навыки остывают быстрее, фокус дает больше энергии.|Habilidades recarregam mais rápido e foco dá energia.|Skills laden schneller, Fokus gibt mehr Energie.|Yetenekler hızlı döner, odak enerji getirir.|Compétences plus rapides, focus rend énergie.|スキルCD短縮、集中でエネルギー増。
meta.survey.body|Earn a modest Star Dust bonus and read the opening waves more safely.|略微提高星尘收益，让开局节奏更稳。|Небольшой бонус пыли и безопасный старт.|Bônus leve de Pó e início mais seguro.|Etwas mehr Staub und sicherer Auftakt.|Az Toz bonusu ve güvenli açılış.|Petit bonus de poussière et ouverture sûre.|星屑少量増加、序盤が安定。
meta.module.body|Start with real upgrade effects, giving each run an early build seed.|开局直接获得三选一效果，让每局更早形成流派。|Начинай с реальных улучшений для ранней сборки.|Comece com efeitos reais para formar build cedo.|Starte mit echten Upgrades als früher Build-Kern.|Erken build için gerçek geliştirmelerle başla.|Commence avec de vrais bonus pour lancer un build.|実際の強化を持って開始し、序盤からビルド形成。
meta.codex.body|Pilot upgrades appear more often, especially in the early choices.|角色专属升级更容易出现，前期选择更稳定。|Улучшения пилота появляются чаще, особенно рано.|Upgrades do piloto aparecem mais, principalmente cedo.|Piloten-Upgrades erscheinen öfter, besonders früh.|Pilot geliştirmeleri erken seçimlerde daha sık çıkar.|Les bonus de pilote apparaissent plus souvent au début.|専用強化が序盤から出やすくなる。
meta.combo.body|High combo speeds up enemy spawns more, and hit recovery is less harsh.|高连击对刷怪加速更明显，受伤断连惩罚更轻。|Высокое комбо сильнее ускоряет спавн, штраф мягче.|Combo alto acelera spawns mais; punição ao ser atingido cai.|Hohe Combos beschleunigen Spawns stärker, Treffer strafen weniger.|Yüksek kombo doğuşu hızlandırır, darbe cezası azalır.|Les hauts combos accélèrent plus les vagues, punition réduite.|高コンボで出現加速、被弾時の損失を軽減。
meta.elite.body|Deal more damage to elite enemies and Bosses without boosting trash clears.|对精英与Boss伤害提高，但不会直接强化普通清怪。|Больше урона элите и боссам, без усиления зачистки мелочи.|Mais dano em elite e Boss, sem acelerar inimigos fracos.|Mehr Schaden gegen Elite und Bosse, nicht gegen Kleinkram.|Elit ve Boss hasarı artar, küçük düşmana değil.|Plus de dégâts aux élites et Boss, pas aux ennemis faibles.|エリートとBossへのダメージ増、雑魚狩りは強化しない。
upgrade.prism.title|Prism Array|多重射击|Призменный массив|Matriz Prisma|Prismenfeld|Prizma Dizisi|Réseau prisme|プリズム配列
upgrade.rail.title|Rail Heart|强力核心|Рельсовое сердце|Coração Rail|Schienenherz|Ray Kalbi|Cœur rail|レール心臓
upgrade.coolant.title|Coolant Lattice|冷却装置|Охлаждающая решетка|Malha de Resfriamento|Kühlgitter|Soğutma Kafesi|Réseau de refroidissement|冷却格子
upgrade.kinetic.title|Kinetic Bloom|机动强化|Кинетический цветок|Florescer Cinético|Kinetikblüte|Kinetik Çiçek|Floraison cinétique|機動開花
upgrade.gravity.title|Gravity Well|拾取范围|Гравиколодец|Poço Gravitacional|Gravitationsbrunnen|Yerçekimi Kuyusu|Puits gravitationnel|重力井
upgrade.vital.title|Vital Shell|生命护盾|Живой панцирь|Casca Vital|Vitalschale|Can Kabuğu|Coque vitale|生命殻
upgrade.leech.title|Repair Seed|修复掉落|Зерно ремонта|Semente de Reparo|Reparatursaat|Onarım Tohumu|Germe de réparation|修理種
upgrade.wisp.title|Moon Wisp|自动浮游炮|Лунный огонек|Fagulha Lunar|Mondirrlicht|Ay Işığı|Feu follet lunaire|月光ウィスプ
upgrade.rift.title|Rift Needle|穿透弹|Игла разлома|Agulha da Fenda|Rissnadel|Yarık İğnesi|Aiguille de faille|裂け目針
upgrade.mirror.title|Mirror Skin|减伤装甲|Зеркальная кожа|Pele Espelhada|Spiegelhaut|Ayna Derisi|Peau miroir|鏡面装甲
upgrade.nova.title|Emergency Capacitor|清弹电容|Конденсатор очистки|Capacitor de Limpeza|Notkondensator|Acil Kapasitör|Condensateur d’urgence|緊急蓄電
upgrade.storm.title|Tactical Overdrive|战术超载|Тактический разгон|Sobrecarga Tática|Taktik-Overdrive|Taktik Aşırı Yük|Survoltage tactique|戦術過給
upgrade.comet.title|Comet Trail|冲刺强化|Кометный след|Rastro de Cometa|Kometenspur|Kuyruklu İz|Traînée comète|彗星軌跡
upgrade.aegis.title|Aegis Bloom|自动回血|Цветение Эгиды|Florescer Égide|Aegisblüte|Kalkan Çiçeği|Floraison Égide|イージス開花
upgrade.echo.title|Quantum Echo|额外射击|Квантовое эхо|Eco Quântico|Quantenecho|Kuantum Yankı|Écho quantique|量子反響
upgrade.chain.title|Chain Relay|连锁中继|Цепное реле|Relé de Cadeia|Kettenrelais|Zincir Rölesi|Relais de chaîne|連鎖中継
upgrade.fractal.title|Fractal Split|分裂棱片|Фрактальный раскол|Divisão Fractal|Fraktalsplitter|Fraktal Bölünme|Fragment fractal|フラクタル分裂
upgrade.solar.title|Flow Core|流派核心|Ядро потока|Núcleo de Fluxo|Flusskern|Akış Çekirdeği|Noyau de flux|フロー核
upgrade.repair.title|Emergency Repair|紧急维修|Экстренный ремонт|Reparo de Emergência|Notreparatur|Acil Onarım|Réparation d’urgence|緊急修理
upgrade.overdrive.title|One-Wave Overdrive|单波过载|Разгон на волну|Overdrive de Uma Onda|Ein-Wellen-Overdrive|Tek Dalga Aşırı Yük|Surcharge une vague|一波過給
upgrade.glass.title|Glass Cannon|玻璃大炮|Стеклянная пушка|Canhão de Vidro|Glaskanone|Cam Top|Canon de verre|ガラス砲
upgrade.bounty.title|Risk Reward|高风险奖励|Награда за риск|Recompensa de Risco|Risiko-Belohnung|Risk Ödülü|Récompense risquée|リスク報酬
upgrade.transmute.title|Bullet Transmute|弹幕转化|Трансмутация пуль|Transmutar Tiros|Kugelwandlung|Mermi Dönüşümü|Transmutation des tirs|弾幕変換
upgrade.map.title|Harmonic Map|升级地图|Гармоническая карта|Mapa Harmônico|Harmoniekarte|Uyum Haritası|Carte harmonique|調和マップ
upgrade.astra.refraction.title|Refraction Lattice|折射阵列|Решетка преломления|Malha de Refração|Brechungsgitter|Kırılım Kafesi|Réseau de réfraction|屈折格子
upgrade.astra.wake.title|Prism Wake|棱光余波|Призменный след|Rastro Prisma|Prismensog|Prizma İzi|Sillage prisme|プリズム航跡
upgrade.vesper.charge.title|Capacitor Spine|蓄能脊柱|Емкостной хребет|Espinha Capacitor|Kondensatorrückgrat|Kapasitör Omurgası|Épine capacitive|蓄電脊柱
upgrade.vesper.fork.title|Split Rail|分裂轨道|Раздвоенная рельса|Trilho Dividido|Splitterschiene|Bölünmüş Ray|Rail divisé|分岐レール
upgrade.kairo.bay.title|Drone Bay|无人机舱|Отсек дронов|Baia Drone|Drohnenbucht|Dron Bölmesi|Baie drone|ドローン庫
upgrade.kairo.sync.title|Swarm Sync|蜂群同步|Синхронизация роя|Sincronia do Enxame|Schwarmsync|Sürü Senkronu|Synchronie d’essaim|群同期
upgrade.sol.bloom.title|Corona Bloom|日冕绽放|Цветение короны|Florescer Corona|Koronenblüte|Korona Çiçeği|Floraison corona|コロナ開花
upgrade.sol.forge.title|Solar Forge|太阳熔炉|Солнечная кузня|Forja Solar|Sonnenesse|Güneş Ocağı|Forge solaire|太陽炉
upgrade.astra.nova.title|Prism Reservoir|蓄光棱镜|Призменный резерв|Reservatório Prisma|Prismenreserve|Prizma Haznesi|Réserve prisme|プリズム蓄光
upgrade.astra.twin.title|Twin Refraction|双相折射|Двойное преломление|Refração Gêmea|Zwillingsbrechung|İkiz Kırılım|Double réfraction|双屈折
upgrade.vesper.judgment.title|Judgment Coil|裁决线圈|Катушка суда|Bobina do Julgamento|Urteilsspule|Hüküm Bobini|Bobine du jugement|審判コイル
upgrade.vesper.sever.title|Sever Line|裂轨余震|Линия отсечения|Linha de Corte|Trennlinie|Kesik Çizgi|Ligne de rupture|断裂線
upgrade.kairo.override.title|Override Matrix|覆写矩阵|Матрица перехвата|Matriz Override|Override-Matrix|Geçersiz Kılma Matrisi|Matrice d’override|上書き行列
upgrade.kairo.relay.title|Relay Protocol|接力协议|Протокол реле|Protocolo Relé|Relaisprotokoll|Röle Protokolü|Protocole relais|中継プロトコル
upgrade.sol.flare.title|Flare Core|耀斑核心|Ядро вспышки|Núcleo de Clarão|Flarekern|Parlama Çekirdeği|Noyau d’éruption|フレア核
upgrade.sol.mantle.title|Radiant Mantle|光冕护层|Сияющая мантия|Manto Radiante|Strahlmantel|Işıltı Örtüsü|Manteau radiant|輝光マントル
upgrade.unknown.title|Unknown|未知升级|Неизвестно|Desconhecido|Unbekannt|Bilinmiyor|Inconnu|未知
upgrade.unknown.body|Effect not shown.|效果未显示。|Эффект не показан.|Efeito não exibido.|Effekt nicht angezeigt.|Etki gösterilmez.|Effet non affiché.|効果未表示。
hud.score|SCORE {0:000000}|分数 {0:000000}|СЧЕТ {0:000000}|SCORE {0:000000}|SCORE {0:000000}|SKOR {0:000000}|SCORE {0:000000}|スコア {0:000000}
hud.score.label|SCORE|分数|СЧЕТ|SCORE|SCORE|SKOR|SCORE|スコア
score.cache.hint|Run rewards add Star Dust after the run|本局奖励会在结算时转成星尘|Награды забега дают пыль после боя|Recompensas da run viram Pó Estelar|Run-Belohnungen geben danach Sternenstaub|Koşu ödülü sonunda Yıldız Tozu verir|Les récompenses donnent de la poussière après la run|報酬は終了時に星屑になる
title.loop|LOOP|循环|ЦИКЛ|LOOP|SCHLEIFE|DÖNGÜ|BOUCLE|ループ
title.fighter|FIGHTER|战机|ИСТРЕБИТЕЛЬ|CAÇA|JÄGER|SAVAŞÇI|CHASSEUR|戦機
pilot.unlock.wave8|Reach wave 8|到达第 8 波|Дойти до волны 8|Alcance onda 8|Welle 8 erreichen|8. dalgaya ulaş|Atteindre vague 8|Wave 8 到達
pilot.unlock.wave16|Reach wave 16|到达第 16 波|Дойти до волны 16|Alcance onda 16|Welle 16 erreichen|16. dalgaya ulaş|Atteindre vague 16|Wave 16 到達
pilot.unlock.wave24|Reach wave 24|到达第 24 波|Дойти до волны 24|Alcance onda 24|Welle 24 erreichen|24. dalgaya ulaş|Atteindre vague 24|Wave 24 到達
capstone.chain|MAX RANK: chain jumps farther and ends with a shard burst.|满级：连锁跳得更远，最后爆出碎片。|МАКС: цепь прыгает дальше и заканчивается взрывом осколков.|MÁX: cadeia salta mais longe e termina em estilhaços.|MAX: Ketten springen weiter und enden mit Splittern.|MAKS: zincir uzağa sıçrar ve parça patlatır.|MAX : la chaîne saute plus loin et finit en éclats.|最大：連鎖が遠くへ跳び、最後に破片爆発。
capstone.fractal|MAX RANK: split shots create a second split.|满级：分裂弹会再分裂一次。|МАКС: осколки делятся второй раз.|MÁX: tiros divididos se dividem de novo.|MAX: Splitter teilen sich ein zweites Mal.|MAKS: bölünen atış ikinci kez bölünür.|MAX : les éclats se divisent une seconde fois.|最大：分裂弾がもう一度分裂。
capstone.astra.refraction|MAX RANK: Astra gains permanent side shots.|满级：星棱获得常驻侧向子弹。|МАКС: Астра получает постоянные боковые выстрелы.|MÁX: Astra ganha tiros laterais fixos.|MAX: Astra erhält dauerhafte Seitenschüsse.|MAKS: Astra kalıcı yan atış kazanır.|MAX : Astra gagne des tirs latéraux permanents.|最大：アストラに常時側面弾追加。
capstone.astra.wake|MAX RANK: Astra skill fires stronger echo shots.|满级：星棱技能追加更强的回声弹。|МАКС: навык Астры дает сильные эхо-выстрелы.|MÁX: habilidade de Astra dispara ecos mais fortes.|MAX: Astras Skill feuert stärkere Echo-Schüsse.|MAKS: Astra yeteneği güçlü yankı atışı yapar.|MAX : la compétence d’Astra tire des échos plus forts.|最大：アストラスキルに強いエコー弾。
capstone.astra.nova|MAX RANK: Astra skill reaches farther and ultimate costs less.|满级：星棱技能范围更远，大招耗能更低。|МАКС: навык Астры дальше, ульта дешевле.|MÁX: habilidade de Astra vai mais longe e ultimate custa menos.|MAX: Astras Skill reicht weiter, Ult kostet weniger.|MAKS: Astra yeteneği uzağa gider, ulti ucuzlar.|MAX : compétence d’Astra plus longue, ultime moins cher.|最大：アストラスキル射程増、奥義コスト減。
capstone.astra.twin|MAX RANK: Astra skill fires a cross beam.|满级：星棱技能会打出十字光束。|МАКС: навык Астры стреляет крестовым лучом.|MÁX: habilidade de Astra dispara feixe em cruz.|MAX: Astras Skill feuert Kreuzstrahl.|MAKS: Astra yeteneği çapraz ışın atar.|MAX : compétence d’Astra tire un rayon en croix.|最大：アストラスキルが十字ビーム化。
capstone.vesper.charge|MAX RANK: Vesper shots leave a delayed spear.|满级：暮轨射击留下延迟星矛。|МАКС: выстрелы Веспер оставляют задержанное копье.|MÁX: tiros de Vesper deixam lança atrasada.|MAX: Vesper-Schüsse lassen verzögerte Speere.|MAKS: Vesper atışı gecikmeli mızrak bırakır.|MAX : les tirs de Vesper laissent une lance retardée.|最大：ヴェスパー弾が遅延槍を残す。
capstone.vesper.fork|MAX RANK: side beams form a steady triangle.|满级：侧向光束形成稳定三角火力。|МАКС: боковые лучи дают стабильный треугольник.|MÁX: feixes laterais formam triângulo estável.|MAX: Seitenstrahlen bilden ein stabiles Dreieck.|MAKS: yan ışınlar sabit üçgen kurar.|MAX : les rayons latéraux forment un triangle stable.|最大：側面ビームが安定した三角射撃に。
capstone.vesper.judgment|MAX RANK: Vesper skill pierces Boss guard and becomes wider.|满级：暮轨技能穿透 Boss 护盾，并且范围更宽。|МАКС: навык Веспер пробивает защиту босса и шире.|MÁX: habilidade de Vesper perfura guarda de chefe e fica maior.|MAX: Vesper-Skill pierct Boss-Wache und wird breiter.|MAKS: Vesper yeteneği boss korumasını deler ve genişler.|MAX : compétence de Vesper perce la garde du boss et s’élargit.|最大：ヴェスパースキルがBoss防御を貫き広がる。
capstone.vesper.sever|MAX RANK: side beams cross into a wider pattern.|满级：侧向光束交叉，覆盖更宽。|МАКС: боковые лучи скрещиваются шире.|MÁX: feixes laterais cruzam e cobrem mais.|MAX: Seitenstrahlen kreuzen breiter.|MAKS: yan ışınlar geniş çaprazlanır.|MAX : les rayons latéraux se croisent plus large.|最大：側面ビームが広く交差。
capstone.kairo.bay|MAX RANK: drones clear bullets around themselves.|满级：无人机会清除自身周围红弹。|МАКС: дроны чистят пули вокруг себя.|MÁX: drones limpam tiros ao redor.|MAX: Drohnen räumen Kugeln um sich.|MAKS: dronlar etrafındaki mermiyi temizler.|MAX : les drones nettoient les tirs autour d’eux.|最大：ドローンが周囲の弾を消す。
capstone.kairo.sync|MAX RANK: drone shots gain an extra lock-on stream.|满级：无人机追加一组锁定弹流。|МАКС: дроны получают доп. поток наведения.|MÁX: drones ganham fluxo extra com mira.|MAX: Drohnen erhalten Extra-Lock-on-Strom.|MAKS: dronlar ekstra kilitli atış alır.|MAX : les drones gagnent un flux verrouillé en plus.|最大：ドローンに追加ロック射撃。
capstone.kairo.override|MAX RANK: skill refunds energy and drone damage increases.|满级：技能返还能量，无人机伤害提高。|МАКС: навык возвращает энергию, дроны бьют сильнее.|MÁX: habilidade devolve energia e drones causam mais dano.|MAX: Skill gibt Energie zurück, Drohnenschaden steigt.|MAKS: yetenek enerji döndürür, dron hasarı artar.|MAX : compétence rend énergie et drones plus forts.|最大：スキルでエネルギー返還、ドローン火力増。
capstone.kairo.relay|MAX RANK: skill refunds more energy and chain shots improve.|满级：技能返还更多能量，连锁射击更强。|МАКС: навык дает больше энергии и усиливает цепь.|MÁX: habilidade devolve mais energia e melhora cadeias.|MAX: Skill gibt mehr Energie und Ketten werden stärker.|MAKS: yetenek daha çok enerji verir, zincir güçlenir.|MAX : compétence rend plus d’énergie et renforce la chaîne.|最大：スキルのエネルギー返還増、連鎖強化。
capstone.sol.bloom|MAX RANK: spread shots cover almost all directions.|满级：散射弹几乎覆盖全方向。|МАКС: веер почти закрывает все стороны.|MÁX: dispersão cobre quase todas direções.|MAX: Streuschüsse decken fast alle Richtungen.|MAKS: saçma neredeyse her yönü kaplar.|MAX : les tirs couvrent presque toutes les directions.|最大：散弾がほぼ全方向を覆う。
capstone.sol.forge|MAX RANK: max energy increases and energy refills.|满级：能量上限提高，并回复大量能量。|МАКС: максимум энергии выше и энергия пополняется.|MÁX: energia máxima sobe e recarrega bastante.|MAX: Max-Energie steigt und füllt sich auf.|MAKS: maksimum enerji artar ve dolar.|MAX : énergie max augmentée et recharge.|最大：最大エネルギー増加、大きく回復。
capstone.sol.flare|MAX RANK: skill pulses jump through nearby enemies.|满级：技能脉冲会在附近敌人之间连锁。|МАКС: импульсы навыка прыгают между врагами.|MÁX: pulsos da habilidade saltam entre inimigos.|MAX: Skill-Pulse springen zwischen Feinden.|MAKS: yetenek darbeleri düşmanlar arasında sıçrar.|MAX : les pulses sautent entre ennemis proches.|最大：スキルパルスが敵の間を連鎖。
capstone.sol.mantle|MAX RANK: skill gives a short shield.|满级：使用技能会获得短暂护盾。|МАКС: навык дает короткий щит.|MÁX: habilidade dá escudo curto.|MAX: Skill gibt kurzen Schild.|MAKS: yetenek kısa kalkan verir.|MAX : la compétence donne un court bouclier.|最大：スキルで短いシールド獲得。
upgrade.prism.body|Weapon volleys gain more shots or lanes.|武器齐射增加子弹或弹道。|Залпы оружия получают больше пуль или линий.|Rajadas da arma ganham mais tiros ou linhas.|Waffensalven erhalten mehr Schüsse oder Bahnen.|Silah salvoları daha çok atış veya hat kazanır.|Les salves gagnent plus de tirs ou de lignes.|武器斉射の弾数または射線が増える。
upgrade.rail.body|Weapon damage increases and fire rate improves slightly.|提高武器伤害，并略微提高射速。|Урон оружия выше, скорострельность немного выше.|Aumenta dano da arma e um pouco a cadência.|Waffenschaden steigt, Feuerrate leicht höher.|Silah hasarı artar, atış hızı biraz yükselir.|Dégâts d’arme augmentés et cadence un peu plus rapide.|武器ダメージ増、連射速度が少し上がる。
upgrade.coolant.body|Fire faster and gain more max energy.|射击更快，能量上限更高。|Стрельба быстрее, максимум энергии выше.|Atire mais rápido e ganhe mais energia máxima.|Schneller feuern und mehr Max-Energie.|Daha hızlı ateş et, maksimum enerji artar.|Tire plus vite et augmente l’énergie max.|連射速度と最大エネルギー増加。
upgrade.kinetic.body|Move faster and dash farther.|移动更快，冲刺更远。|Движение быстрее, рывок дальше.|Mova mais rápido e dê dash mais longe.|Schneller bewegen und weiter sprinten.|Daha hızlı hareket et, daha uzağa atıl.|Déplacement plus rapide et dash plus long.|移動が速く、ダッシュが長くなる。
upgrade.gravity.body|Pickups fly to you from farther away. Enemies slow slightly.|拾取范围更远，敌人略微变慢。|Сбор летит издалека, враги немного медленнее.|Coletas vêm de mais longe. Inimigos ficam um pouco lentos.|Pickups kommen von weiter weg. Feinde werden etwas langsamer.|Toplamalar uzaktan gelir. Düşman biraz yavaşlar.|Les objets viennent de plus loin. Les ennemis ralentissent un peu.|拾得物の吸引範囲増加、敵が少し遅くなる。
upgrade.vital.body|Increase max hull and heal now.|提高生命上限，并立即回血。|Максимум корпуса выше и ремонт сразу.|Aumenta casco máximo e cura agora.|Max-Hülle erhöhen und sofort heilen.|Maks gövde artar ve şimdi iyileştirir.|Augmente la coque max et soigne maintenant.|最大耐久増加、即時回復。
upgrade.leech.body|Kills are more likely to drop repair pickups. Elites have a higher chance.|击杀更容易掉落修复道具，精英敌人的概率更高。|Убийства чаще дают ремонт. У элиты шанс выше.|Abates soltam mais reparos. Elites têm chance maior.|Kills lassen eher Reparatur fallen. Eliten haben höhere Chance.|Öldürmeler daha çok onarım düşürür; elitlerde şans artar.|Les kills lâchent plus souvent des réparations, surtout les élites.|撃破時の修理ドロップ率上昇。エリートはさらに高い。
upgrade.wisp.body|Add an orbiting drone that auto-fires at nearby enemies.|增加一个环绕无人机，自动攻击附近敌人。|Добавляет дрона, который сам стреляет рядом.|Adiciona drone orbital que atira em inimigos perto.|Orbit-Drohne feuert automatisch auf nahe Feinde.|Dönen dron yakın düşmana otomatik ateş eder.|Ajoute un drone orbital qui tire sur les ennemis proches.|周回ドローンが近くの敵を自動攻撃。
upgrade.rift.body|Weapon shots fly faster and pierce more.|武器子弹更快，并增加穿透。|Выстрелы оружия летят быстрее и лучше пробивают.|Tiros da arma voam mais rápido e perfuram mais.|Waffenschüsse fliegen schneller und durchdringen mehr.|Silah atışları hızlanır ve daha çok deler.|Les tirs de l'arme vont plus vite et percent mieux.|武器弾が速くなり、貫通が増える。
upgrade.mirror.body|Take less damage from hits.|降低受到的伤害。|Получаешь меньше урона.|Receba menos dano.|Du nimmst weniger Schaden.|Daha az hasar alırsın.|Réduit les dégâts subis.|受けるダメージを減らす。
upgrade.nova.body|Ultimate costs less energy and max energy increases.|大招耗能降低，能量上限提高。|Ульта дешевле, максимум энергии выше.|Ultimate custa menos e energia máxima sobe.|Ult kostet weniger Energie und Max-Energie steigt.|Ulti daha az enerji harcar, maksimum enerji artar.|L’ultime coûte moins et l’énergie max augmente.|奥義コスト減、最大エネルギー増加。
upgrade.storm.body|Pilot skill cooldown is shorter. Using skill fires radial shots.|角色技能冷却缩短。使用技能时发射环形子弹。|Откат навыка меньше. При навыке выпускается круг выстрелов.|A habilidade recarrega mais rápido. Ao usar, dispara tiros em círculo.|Skill lädt schneller. Beim Einsatz feuert er Rundumschüsse.|Yetenek beklemesi kısalır. Kullanırken halka atışları yapar.|La compétence recharge plus vite. À l'usage, elle tire en cercle.|スキルの待ち時間短縮。使用時に円形弾を撃つ。
upgrade.comet.body|Dash deals more damage and clears more bullets.|冲刺伤害更高，并清除更多红弹。|Рывок бьет сильнее и чистит больше пуль.|Dash causa mais dano e limpa mais tiros.|Sprint macht mehr Schaden und räumt mehr Kugeln.|Atılma daha çok hasar verir ve daha çok mermi temizler.|Le dash inflige plus et nettoie plus de tirs.|ダッシュ火力増、弾消し範囲増加。
upgrade.aegis.body|After avoiding damage for a while, slowly heal.|一段时间不受伤后，缓慢回血。|Если не получать урон, корпус медленно чинится.|Sem dano por um tempo, cura devagar.|Nach etwas Zeit ohne Schaden langsam heilen.|Bir süre hasar almazsan yavaş iyileşirsin.|Après un moment sans dégâts, soin lent.|しばらく被弾しないと少しずつ回復。
upgrade.echo.body|Weapon shots can echo into an extra piercing shot.|武器射击有概率回响出额外穿透弹。|Выстрелы оружия могут создать доп. пробивной отзвук.|Tiros da arma podem ecoar em um perfurante extra.|Waffenschüsse können als extra Durchschuss nachhallen.|Silah atışları ekstra delici yankı çıkarabilir.|Les tirs de l'arme peuvent créer un écho perforant.|武器弾が追加の貫通反響弾を出すことがある。
upgrade.chain.body|Weapon hits can jump damage to nearby enemies.|武器命中可把伤害跳到附近敌人。|Попадания могут прыгнуть к соседним врагам.|Acertos podem saltar dano para inimigos perto.|Treffer können Schaden auf nahe Feinde springen lassen.|İsabet yakındaki düşmana hasar sıçratabilir.|Les impacts peuvent sauter vers les ennemis proches.|命中が近くの敵へダメージ連鎖。
upgrade.fractal.body|Kills create small split shots.|击杀会生成小型分裂弹。|Убийства создают мелкие осколки.|Abates criam tiros divididos pequenos.|Kills erzeugen kleine Splittergeschosse.|Öldürme küçük bölünen atış çıkarır.|Les kills créent de petits tirs fragmentés.|撃破で小さな分裂弾が出る。
upgrade.solar.body|Increase overall damage and skill-window burst damage.|提高整体伤害和技能窗口爆发伤害。|Повышает общий урон и взрывной урон окна навыка.|Aumenta dano geral e explosão na janela da habilidade.|Erhöht Gesamtschaden und Burst im Skill-Fenster.|Genel hasarı ve yetenek penceresi patlama hasarını artırır.|Augmente les dégâts globaux et le burst après compétence.|全体ダメージとスキル後の瞬間火力が増える。
upgrade.repair.body|Heal now and increase max hull slightly.|立即回血，并小幅提高生命上限。|Лечит сейчас и немного повышает корпус.|Cura agora e aumenta um pouco o casco.|Sofort heilen und Max-Hülle leicht erhöhen.|Şimdi iyileştirir, maks gövde az artar.|Soigne maintenant et augmente un peu la coque.|即時回復し、最大耐久を少し上げる。
upgrade.overdrive.body|Next wave: deal much more damage. Gain energy now.|下一波伤害大幅提高，并立即获得能量。|Следующая волна: намного больше урона. Энергия сейчас.|Próxima onda: muito mais dano. Ganhe energia agora.|Nächste Welle: viel mehr Schaden. Jetzt Energie erhalten.|Sonraki dalga çok daha fazla hasar. Şimdi enerji kazan.|Prochaine vague : beaucoup plus de dégâts. Énergie immédiate.|次Waveは大火力。今エネルギー獲得。
upgrade.glass.body|Increase damage, but reduce max hull.|提高伤害，但降低生命上限。|Урон выше, максимум корпуса ниже.|Aumenta dano, mas reduz casco máximo.|Mehr Schaden, aber weniger Max-Hülle.|Hasar artar ama maks gövde düşer.|Augmente les dégâts mais réduit la coque max.|火力増、最大耐久低下。
upgrade.bounty.body|Next wave has more enemies and more experience drops.|下一波敌人更多，经验掉落更多。|Следующая волна: больше врагов и больше опыта.|Próxima onda tem mais inimigos e mais experiência.|Nächste Welle hat mehr Feinde und mehr Erfahrung.|Sonraki dalgada daha çok düşman ve deneyim düşer.|Prochaine vague avec plus d'ennemis et d'expérience.|次Waveは敵が増え、経験値ドロップも増える。
upgrade.transmute.body|Clear enemy bullets now and gain energy.|立刻清除敌方子弹，并获得能量。|Чистит пули сейчас и дает энергию.|Limpa tiros inimigos agora e dá energia.|Feindkugeln jetzt räumen und Energie erhalten.|Düşman mermisini temizle ve enerji kazan.|Nettoie les tirs ennemis et donne énergie.|敵弾を消し、エネルギー獲得。
upgrade.map.body|Gain one extra reroll on future upgrade screens.|之后每次升级界面多一次重抽。|На будущих выборах один доп. реролл.|Ganha um reroll extra nas próximas escolhas.|Künftige Wahlen erhalten einen Neuwurf.|Gelecek seçimlerde ekstra yenileme.|Une relance en plus aux prochains choix.|以後の強化画面で再抽選+1。
upgrade.astra.refraction.body|Astra fires extra side shots. Skill fires more beams.|星棱追加侧向子弹。技能光束数量增加。|Астра стреляет боковыми пулями. Навык дает больше лучей.|Astra ganha tiros laterais. A habilidade dispara mais feixes.|Astra feuert Seitenschüsse. Der Skill feuert mehr Strahlen.|Astra yan atış kazanır. Yetenek daha çok ışın atar.|Astra tire sur les côtés. La compétence tire plus de rayons.|アストラに側面弾追加。スキルのビーム数増加。
upgrade.astra.wake.body|Astra shots deal more damage and fire faster. Skill lasts longer.|星棱子弹伤害更高、射速更快。技能持续更久。|Выстрелы Астры сильнее и быстрее. Навык дольше.|Tiros de Astra causam mais dano e saem mais rápido. Habilidade dura mais.|Astra-Schüsse sind stärker und schneller. Skill dauert länger.|Astra atışı güçlü ve hızlı olur. Yetenek uzar.|Tirs d’Astra plus forts et plus rapides. Compétence plus longue.|アストラ弾の火力と速度増加。スキル延長。
upgrade.vesper.charge.body|Vesper shots deal more damage. Skill line hits harder.|暮轨子弹伤害提高。技能伤害线更强。|Выстрелы Веспер сильнее. Линия навыка бьет сильнее.|Tiros de Vesper causam mais dano. Linha da habilidade bate mais.|Vesper-Schüsse stärker. Skill-Linie trifft härter.|Vesper atışı daha güçlü. Yetenek çizgisi sert vurur.|Tirs de Vesper plus forts. Ligne de compétence plus puissante.|ヴェスパー弾とスキル線の火力増加。
upgrade.vesper.fork.body|Vesper shots add narrow side beams.|暮轨射击追加两侧细光束。|Выстрелы Веспер добавляют боковые лучи.|Tiros de Vesper ganham feixes laterais.|Vesper-Schüsse erhalten Seitenstrahlen.|Vesper atışı yan ışın ekler.|Les tirs de Vesper ajoutent des rayons latéraux.|ヴェスパー弾に側面ビーム追加。
upgrade.kairo.bay.body|Kairo gains more drones. Skill commands more drones at once.|环序获得更多无人机。技能一次指挥更多无人机。|Кайро получает больше дронов. Навык командует ими вместе.|Kairo ganha mais drones. Habilidade comanda mais drones.|Kairo erhält mehr Drohnen. Skill befehligt mehr auf einmal.|Kairo daha çok dron alır. Yetenek daha çoğunu yönetir.|Kairo gagne plus de drones. La compétence en commande plus.|カイロのドローン増加。スキルでより多く指揮。
upgrade.kairo.sync.body|Drones fire faster. Skill volley shoots more bullets.|无人机射击更快。技能齐射子弹更多。|Дроны стреляют быстрее. Навык выпускает больше пуль.|Drones atiram mais rápido. Habilidade dispara mais balas.|Drohnen feuern schneller. Skill-Salve hat mehr Kugeln.|Dronlar hızlı ateş eder. Yetenek daha çok mermi atar.|Drones plus rapides. La salve de compétence tire plus.|ドローン連射増加。スキル斉射の弾数増加。
upgrade.sol.bloom.body|Sol spread fires more bullets. Skill area becomes larger.|日冕散射弹数增加。技能范围更大。|Веер Сола стреляет больше. Зона навыка шире.|Sol dispara mais balas. Área da habilidade aumenta.|Sol feuert mehr Streuschüsse. Skill-Fläche größer.|Sol daha çok saçma atar. Yetenek alanı büyür.|Sol tire plus de projectiles. Zone de compétence plus grande.|ソル散弾数とスキル範囲増加。
upgrade.sol.forge.body|Sol gains more energy. Ultimate and skill become easier to use.|日冕获得更多能量。大招和技能更容易释放。|Сол получает больше энергии. Ульта и навык легче использовать.|Sol ganha mais energia. Ultimate e habilidade ficam mais fáceis.|Sol erhält mehr Energie. Ult und Skill werden leichter nutzbar.|Sol daha çok enerji alır. Ulti ve yetenek kolaylaşır.|Sol gagne plus d’énergie. Ultime et compétence plus faciles.|ソルのエネルギー増加、奥義とスキルが使いやすい。
upgrade.astra.nova.body|Astra skill reaches farther. Ultimate costs less energy.|星棱技能范围更远。大招耗能更低。|Навык Астры дальше. Ульта дешевле.|Habilidade de Astra vai mais longe. Ultimate custa menos.|Astra-Skill reicht weiter. Ult kostet weniger.|Astra yeteneği uzağa gider. Ulti ucuzlar.|Compétence d’Astra plus longue. Ultime moins cher.|アストラスキル射程増、奥義コスト減。
upgrade.astra.twin.body|Astra skill fires a second ring and creates more split shots.|星棱技能追加第二圈，并生成更多分裂弹。|Навык Астры дает второе кольцо и больше осколков.|Habilidade de Astra ganha segundo anel e mais divisões.|Astra-Skill feuert zweiten Ring und mehr Splitter.|Astra yeteneği ikinci halka ve daha çok bölünme atar.|Compétence d’Astra avec second anneau et plus d’éclats.|アストラスキルに第2リングと分裂弾追加。
upgrade.vesper.judgment.body|Vesper skill line becomes wider and costs less energy.|暮轨技能伤害线更宽，并且耗能更低。|Линия навыка Веспер шире и дешевле.|Linha da habilidade de Vesper fica maior e custa menos.|Vesper-Skilllinie wird breiter und billiger.|Vesper yetenek çizgisi genişler ve ucuzlar.|Ligne de Vesper plus large et moins chère.|ヴェスパースキル線が太くなり、コスト減。
upgrade.vesper.sever.body|Vesper skill creates extra parallel beams.|暮轨技能生成额外平行光束。|Навык Веспер создает доп. параллельные лучи.|Habilidade de Vesper cria feixes paralelos extras.|Vesper-Skill erzeugt parallele Zusatzstrahlen.|Vesper yeteneği ekstra paralel ışın çıkarır.|Compétence de Vesper crée des rayons parallèles.|ヴェスパースキルに平行ビーム追加。
upgrade.kairo.override.body|Kairo skill fires extra command shots. Drone shots deal more damage.|环序技能追加指令弹。无人机伤害提高。|Навык Кайро дает доп. выстрелы. Дроны бьют сильнее.|Habilidade de Kairo dispara tiros extras. Drones causam mais dano.|Kairo-Skill feuert Extraschüsse. Drohnen verursachen mehr Schaden.|Kairo yeteneği ekstra komut atışı yapar. Dron hasarı artar.|Compétence de Kairo tire en plus. Drones plus forts.|カイロスキルに追加弾、ドローン火力増。
upgrade.kairo.relay.body|Kairo skill refunds energy and drone shots can chain.|环序技能返还能量，无人机射击可以连锁。|Навык Кайро возвращает энергию, дроны могут цепляться.|Habilidade de Kairo devolve energia e drones encadeiam.|Kairo-Skill gibt Energie zurück, Drohnen können ketten.|Kairo yeteneği enerji döndürür, dron atışı zincirlenir.|Compétence de Kairo rend énergie et les drones chaînent.|カイロスキルがエネルギー返還、ドローンが連鎖。
upgrade.sol.flare.body|Sol skill deals more damage and clears a larger area.|日冕技能伤害更高，清场范围更大。|Навык Сола бьет сильнее и чистит шире.|Habilidade de Sol causa mais dano e limpa área maior.|Sol-Skill macht mehr Schaden und räumt größere Fläche.|Sol yeteneği daha çok hasar verir ve alan temizler.|Compétence de Sol plus forte et zone nettoyée plus large.|ソルスキル火力と清場範囲増加。
upgrade.sol.mantle.body|Sol gains more hull and longer invulnerability after using skill.|日冕生命更高，使用技能后无敌时间更久。|У Сола больше корпуса и дольше неуязвимость после навыка.|Sol ganha mais casco e invulnerabilidade maior após habilidade.|Sol erhält mehr Hülle und längere Unverwundbarkeit nach Skill.|Sol gövde kazanır, yetenek sonrası dokunulmazlık uzar.|Sol gagne plus de coque et d’invulnérabilité après compétence.|ソル耐久増、スキル後の無敵時間延長。
pilot.nyx.name|Nyx|夜幕|Никс|Nyx|Nyx|Nyx|Nyx|ニクス
pilot.nyx.body|Curved blade shots. Skill pulls and slows enemies, then bursts.|弧形重力刃。技能牵引并减速敌人，然后爆发。|Изогнутые клинки. Навык тянет и замедляет врагов, затем взрывается.|Lâminas curvas. A habilidade puxa, desacelera e explode.|Gebogene Klingen. Der Skill zieht und verlangsamt Feinde, dann platzt er.|Kavisli bıçaklar. Yetenek düşmanı çeker, yavaşlatır, sonra patlar.|Lames courbes. La compétence attire, ralentit puis explose.|曲がる重力刃。スキルは敵を引き寄せ、遅くし、爆発。
pilot.nyx.weapon|Gravity Blades|重力刃|Гравиклинки|Lâminas Grávitas|Gravitationsklingen|Yerçekimi Bıçakları|Lames gravité|重力刃
pilot.rook.name|Rook|壁垒|Ладья|Rook|Rook|Kale|Rook|ルーク
pilot.rook.body|Slow heavy shells. Skill blocks bullets, heals, and pushes enemies away.|慢速重炮。技能挡红弹、回血，并推开敌人。|Медленные тяжелые снаряды. Навык блокирует пули, лечит и отталкивает.|Obuses lentos. Habilidade bloqueia tiros, cura e empurra inimigos.|Langsame schwere Granaten. Skill blockt Kugeln, heilt und stößt Feinde weg.|Yavaş ağır mermi. Yetenek mermi bloklar, iyileştirir ve iter.|Obus lourds lents. La compétence bloque, soigne et repousse.|低速重砲。スキルは弾を防ぎ、回復し、敵を押す。
pilot.rook.weapon|Siege Shell|攻城重炮|Осадный снаряд|Projétil de Cerco|Belagerungsgranate|Kuşatma Mermisi|Obus de siège|攻城弾
pilot.lyra.name|Lyra|弦歌|Лира|Lyra|Lyra|Lyra|Lyra|ライラ
pilot.lyra.body|Rhythm volleys. Every third volley is stronger. Skill sends pulse rings.|节拍齐射。每第三次射击更强。技能释放脉冲环。|Ритм-залпы. Каждый третий сильнее. Навык пускает кольца.|Rajadas rítmicas. Cada terceira é mais forte. Habilidade solta anéis.|Rhythmus-Salven. Jede dritte ist stärker. Skill sendet Pulsringe.|Ritim salvoları. Her üçüncü daha güçlü. Yetenek halka yollar.|Salves rythmiques. Chaque troisième est plus forte. Compétence en anneaux.|リズム斉射。3回目が強い。スキルはパルスリング。
pilot.lyra.weapon|Pulse Chord|脉冲和弦|Пульс-аккорд|Acorde Pulsante|Impulsakkord|Darbe Akoru|Accord pulsé|パルス和音
pilot.orion.name|Orion|猎户|Орион|Orion|Orion|Orion|Orion|オリオン
pilot.orion.body|Precise piercing spears. Skill marks a target and fires extra spears.|精准穿透星矛。技能标记目标并追加星矛。|Точные пробивающие копья. Навык метит цель и пускает доп. копья.|Lanças precisas e perfurantes. Habilidade marca e lança extras.|Präzise Piercing-Speere. Skill markiert Ziel und feuert Extraspeere.|Keskin delici mızraklar. Yetenek hedef işaretler ve ekstra mızrak atar.|Lances précises perforantes. La compétence marque et ajoute des lances.|精密貫通槍。スキルで標的を付け、追加槍を撃つ。
pilot.orion.weapon|Comet Spear|彗星矛|Копье кометы|Lança Cometa|Kometenspeer|Kuyruklu Mızrak|Lance comète|彗星槍
pilot.unlock.nyx|Wave {0}/{1} · Combo {2}/{3} · Absorb {4}/{5}|波次 {0}/{1} · 连击 {2}/{3} · 清弹 {4}/{5}|Волна {0}/{1} · комбо {2}/{3} · очистка {4}/{5}|Onda {0}/{1} · combo {2}/{3} · absorção {4}/{5}|Welle {0}/{1} · Kombo {2}/{3} · Clear {4}/{5}|Dalga {0}/{1} · kombo {2}/{3} · temizleme {4}/{5}|Vague {0}/{1} · combo {2}/{3} · absorption {4}/{5}|Wave {0}/{1} · コンボ {2}/{3} · 吸収 {4}/{5}
pilot.unlock.rook|Wave {0}/{1} · Dust {2}/{3} · Runs {4}/{5}|波次 {0}/{1} · 星尘 {2}/{3} · 出航 {4}/{5}|Волна {0}/{1} · пыль {2}/{3} · забеги {4}/{5}|Onda {0}/{1} · poeira {2}/{3} · runs {4}/{5}|Welle {0}/{1} · Staub {2}/{3} · Läufe {4}/{5}|Dalga {0}/{1} · toz {2}/{3} · sefer {4}/{5}|Vague {0}/{1} · poussière {2}/{3} · runs {4}/{5}|Wave {0}/{1} · 星屑 {2}/{3} · 出撃 {4}/{5}
pilot.unlock.lyra|Wave {0}/{1} · Drops {2}/{3} · Clean {4}/{5}|波次 {0}/{1} · 拾取 {2}/{3} · 无伤波 {4}/{5}|Волна {0}/{1} · сбор {2}/{3} · чисто {4}/{5}|Onda {0}/{1} · coletas {2}/{3} · limpas {4}/{5}|Welle {0}/{1} · Pickups {2}/{3} · sauber {4}/{5}|Dalga {0}/{1} · toplama {2}/{3} · temiz {4}/{5}|Vague {0}/{1} · collectes {2}/{3} · parfait {4}/{5}|Wave {0}/{1} · 収集 {2}/{3} · 無傷 {4}/{5}
pilot.unlock.orion|Wave {0}/{1} · Boss {2}/{3} · Kills {4}/{5}|波次 {0}/{1} · Boss {2}/{3} · 击杀 {4}/{5}|Волна {0}/{1} · босс {2}/{3} · убийства {4}/{5}|Onda {0}/{1} · chefes {2}/{3} · abates {4}/{5}|Welle {0}/{1} · Boss {2}/{3} · Kills {4}/{5}|Dalga {0}/{1} · boss {2}/{3} · öldürme {4}/{5}|Vague {0}/{1} · Boss {2}/{3} · élim. {4}/{5}|Wave {0}/{1} · Boss {2}/{3} · 撃破 {4}/{5}
ultimate.nyx|Singularity Collapse|奇点坍缩|Коллапс сингулярности|Colapso Singular|Singularitätskollaps|Tekillik Çöküşü|Effondrement singulier|特異点崩壊
ultimate.rook|Citadel Quake|城垒震击|Дрожь цитадели|Sismo Cidadela|Zitadellenbeben|Hisar Sarsıntısı|Séisme citadelle|城塞震撃
ultimate.lyra|Encore Halo|返场光环|Бис-ореол|Halo Encore|Encore-Halo|Encore Halkası|Halo rappel|アンコール環
ultimate.orion|Starfall Verdict|星陨裁定|Звездный приговор|Veredito Estelar|Sternfall-Urteil|Yıldız Hükmü|Verdict d’étoiles|星落裁定
tactical.nyx.name|Gravity Snare|重力陷阱|Гравиловушка|Armadilha Grávita|Gravitationsfalle|Yerçekimi Tuzağı|Piège gravité|重力罠
tactical.nyx.tip|Pull enemies together, slow them, then burst.|牵引并减速敌人，然后爆发。|Стягивает врагов, замедляет, затем взрывается.|Puxa inimigos, desacelera e explode.|Zieht Feinde zusammen, verlangsamt und platzt.|Düşmanı toplar, yavaşlatır, sonra patlar.|Regroupe les ennemis, ralentit puis explose.|敵を寄せて遅くし、最後に爆発。
tactical.rook.name|Bulwark Drive|壁垒推进|Прорыв щитом|Avanço Baluarte|Bollwerkstoß|Siper Sürüşü|Poussée rempart|防壁突進
tactical.rook.tip|Block red bullets, heal, and push enemies away.|挡红弹、回血，并推开敌人。|Блокирует пули, лечит и отталкивает.|Bloqueia tiros, cura e empurra.|Blockt rote Kugeln, heilt und stößt weg.|Kırmızı mermi bloklar, iyileştirir ve iter.|Bloque les tirs rouges, soigne et repousse.|赤弾を防ぎ、回復し、敵を押す。
tactical.lyra.name|Beat Pulse|节拍脉冲|Ритм-пульс|Pulso de Batida|Taktpuls|Ritim Darbesi|Pulse rythmique|ビートパルス
tactical.lyra.tip|Pulse rings damage enemies around you.|脉冲环伤害身边敌人。|Кольца наносят урон вокруг тебя.|Anéis causam dano ao redor.|Pulsringe schaden Feinden um dich.|Halkalar çevredeki düşmana hasar verir.|Les anneaux blessent autour de toi.|周囲の敵にリングでダメージ。
tactical.orion.name|Deadeye Mark|死眼标记|Метка снайпера|Marca Certeira|Todesblick-Marke|Keskin Göz İşareti|Marque de tireur|死眼マーク
tactical.orion.tip|Mark a target and fire piercing spears.|标记目标，并发射穿透星矛。|Мечает цель и пускает пробивающие копья.|Marca alvo e dispara lanças perfurantes.|Markiert ein Ziel und feuert Piercing-Speere.|Hedef işaretler ve delici mızrak atar.|Marque une cible et tire des lances perforantes.|標的を付け、貫通槍を撃つ。
boss.mirror.name|Mirror Eidolon|镜像幻体|Зеркальный эйдолон|Eidolon Espelho|Spiegel-Eidolon|Ayna Eidolon|Éidolon miroir|鏡像幻体
boss.tempest.name|Tempest Wheel|风暴轮|Колесо бури|Roda Tempestade|Sturmrad|Fırtına Çarkı|Roue tempête|嵐の輪
boss.bastion.name|Bastion Gate|堡垒门|Врата бастиона|Portão Bastião|Bastiontor|Hisar Kapısı|Porte bastion|要塞門
boss.serpent.name|Coil Serpent|盘蛇|Змей-кольцо|Serpente Espiral|Schlangenspirale|Sarmal Yılan|Serpent spirale|螺旋蛇
boss.oracle.name|Oracle Needle|神谕针|Игла оракула|Agulha Oráculo|Orakelnadel|Kahin İğnesi|Aiguille oracle|神託針
upgrade.pulse.title|Pulse Magazine|脉冲弹匣|Пульс-магазин|Carregador Pulso|Impulsmagazin|Darbe Şarjörü|Chargeur pulsé|パルスマガジン
upgrade.pulse.body|Each volley adds small extra shots.|每次齐射追加小型子弹。|Каждый залп добавляет мелкие выстрелы.|Cada salva adiciona tiros pequenos.|Jede Salve fügt kleine Schüsse hinzu.|Her salvo küçük ekstra atış ekler.|Chaque salve ajoute de petits tirs.|斉射ごとに小型弾を追加。
upgrade.execution.title|Execution Mark|处决标记|Метка казни|Marca de Execução|Exekutionsmarke|İnfaz İşareti|Marque d’exécution|処刑マーク
upgrade.execution.body|Low-health, overheated, or guard-broken enemies take more damage.|低血、过热或破盾敌人受到更多伤害。|Враги с малым здоровьем, перегревом или сломанной защитой получают больше урона.|Inimigos fracos, superaquecidos ou sem guarda sofrem mais dano.|Feinde mit wenig Leben, Überhitzung oder gebrochener Wache nehmen mehr Schaden.|Düşük can, aşırı ısı veya kırık koruma daha çok hasar alır.|Ennemis faibles, surchauffés ou garde brisée subissent plus.|低耐久、過熱、防御破壊の敵に追加ダメージ。
upgrade.stasis.title|Stasis Field|停滞力场|Поле стазиса|Campo de Estase|Stasisfeld|Durgunluk Alanı|Champ de stase|停滞フィールド
upgrade.stasis.body|Enemy bullets and dash enemies move slower.|敌方子弹和突进敌人变慢。|Вражеские пули и рывки медленнее.|Tiros e avanços inimigos ficam lentos.|Feindkugeln und Anstürme werden langsamer.|Düşman mermisi ve hücumu yavaşlar.|Tirs et charges ennemis ralentissent.|敵弾と突進が遅くなる。
upgrade.magnet.title|Magnetized Core|磁化核心|Магнитное ядро|Núcleo Magnetizado|Magnetkern|Mıknatıs Çekirdek|Noyau aimanté|磁化コア
upgrade.magnet.body|Pickups pull from farther away and can clear nearby bullets.|拾取物吸得更远，并可能清除附近红弹。|Сбор тянется дальше и может чистить пули рядом.|Coletas vêm de longe e podem limpar tiros perto.|Pickups ziehen weiter und können nahe Kugeln räumen.|Toplamalar uzaktan gelir ve yakın mermiyi temizleyebilir.|Collectes attirées plus loin et peuvent nettoyer près.|拾得物を遠くから吸い、近くの弾を消すことがある。
upgrade.ricochet.title|Ricochet Matrix|回弹矩阵|Матрица рикошета|Matriz Ricochete|Abprallmatrix|Sekme Matrisi|Matrice ricochet|跳弾マトリクス
upgrade.ricochet.body|Hits can fire a small shard at another enemy.|命中可向另一个敌人弹出小碎片。|Попадания могут послать осколок в другого врага.|Acertos podem lançar estilhaço em outro inimigo.|Treffer können Splitter auf anderen Feind feuern.|İsabet başka düşmana küçük parça atabilir.|Les impacts peuvent lancer un éclat vers un autre ennemi.|命中が別の敵へ小さな破片を飛ばす。
rebound.proc|REBOUND|反弹|ОТРАЖЕНО|REBATE|ABPRALL|SEKME|RENVOI|反射
upgrade.seeker.title|Seeker Missiles|跟踪导弹|Самонаводящиеся ракеты|Mísseis Guiados|Suchraketen|Güdümlü Füzeler|Missiles guidés|追尾ミサイル
upgrade.seeker.body|Fires homing missiles on a short cooldown.|短冷却发射跟踪导弹。|Периодически выпускает самонаводящиеся ракеты.|Dispara mísseis teleguiados com recarga curta.|Feuert Suchraketen mit kurzer Abklingzeit.|Kısa beklemeyle güdümlü füze atar.|Tire des missiles guidés avec une courte recharge.|短い間隔で追尾ミサイルを撃つ。
upgrade.rebound.title|Rebound Shield|反弹护盾|Отражающий щит|Escudo Rebate|Rückprallschild|Sekme Kalkanı|Bouclier renvoi|反射シールド
upgrade.rebound.body|When hit, can turn that red bullet into your shot.|被击中时，有概率把这颗红弹变成我方子弹。|При попадании может превратить красную пулю в ваш выстрел.|Ao ser atingido, pode transformar o tiro vermelho em seu disparo.|Kann bei Treffer die rote Kugel in deinen Schuss wandeln.|Vurulunca kırmızı mermiyi senin atışına çevirebilir.|Quand tu es touché, peut renvoyer ce tir rouge pour toi.|被弾時、赤弾を自分の弾に変えることがある。
upgrade.shadow.title|Shadow Clones|影子分身|Теневые копии|Clones Sombra|Schattenklone|Gölge Klonlar|Clones d'ombre|影分身
upgrade.shadow.body|Your volleys can create side ghost shots.|齐射时有概率从侧面生成影子弹。|Залпы могут создавать боковые призрачные выстрелы.|Rajadas podem criar tiros fantasma laterais.|Salven können seitliche Geisterschüsse erzeugen.|Salvolar yandan hayalet atış çıkarabilir.|Les salves peuvent créer des tirs fantômes latéraux.|連射時、側面から影弾が出ることがある。
upgrade.slug.title|Heavy Slug|重型巨弹|Тяжелый снаряд|Projétil Pesado|Schweres Geschoss|Ağır Mermi|Projectile lourd|重弾
upgrade.slug.body|Fires a slow giant bullet with high damage.|发射低速高伤害巨弹。|Выпускает медленный большой снаряд с высоким уроном.|Dispara um projétil gigante lento e forte.|Feuert eine langsame große Kugel mit hohem Schaden.|Yavaş ama güçlü dev mermi atar.|Tire une grosse balle lente à dégâts élevés.|遅いが高威力の巨大弾を撃つ。
upgrade.pinball.title|Pinball Rounds|弹球弹丸|Пинбол-снаряды|Balas Pinball|Flippergeschosse|Langırt Mermileri|Balles flipper|ピンボール弾
upgrade.pinball.body|Some shots bounce off arena edges.|部分我方子弹会从战场边缘反弹。|Некоторые выстрелы отскакивают от краев арены.|Alguns tiros quicam nas bordas da arena.|Einige Schüsse prallen an Arenarändern ab.|Bazı atışlar arena kenarından seker.|Certains tirs rebondissent sur les bords.|一部の弾が戦場の端で跳ね返る。
upgrade.gyro.title|Gyro Stabilizer|转向稳定器|Гиростабилизатор|Giro Estabilizador|Gyrostabilisator|Jiro Dengeleyici|Gyrostabilisateur|ジャイロ安定器
upgrade.gyro.body|Auto-lock turns faster and fire rate slightly increases.|自动锁敌转向更快，射速小幅提高。|Автозахват быстрее поворачивает, темп огня чуть выше.|A trava automática gira mais rápido e atira um pouco mais.|Auto-Lock dreht schneller, Feuerrate steigt leicht.|Oto kilit hızlı döner, atış hızı az artar.|Le verrouillage tourne plus vite et tire un peu plus vite.|自動ロックの旋回と連射が少し上がる。
upgrade.thrusters.title|Vector Thrusters|矢量推进|Векторные двигатели|Propulsores Vetoriais|Vektorschub|Vektör İticiler|Propulseurs vectoriels|ベクタースラスター
upgrade.thrusters.body|Move faster and shorten dash cooldown.|移速更快，冲刺冷却更短。|Движение быстрее, откат рывка короче.|Move mais rápido e reduz a recarga do dash.|Bewegung schneller, Dash-Abklingzeit kürzer.|Daha hızlı hareket, atılma beklemesi kısalır.|Déplacement plus rapide, dash plus souvent.|移動が速くなり、ダッシュ待ちが短くなる。
upgrade.nyx.orbit.title|Void Orbit|虚空轨道|Пустотная орбита|Órbita Vazia|Leerenorbit|Boşluk Yörüngesi|Orbite vide|虚空軌道
upgrade.nyx.orbit.body|Nyx fires more gravity blades.|夜幕发射更多重力刃。|Никс выпускает больше гравиклинков.|Nyx lança mais lâminas.|Nyx feuert mehr Schwerkraftklingen.|Nyx daha çok yerçekimi bıçağı atar.|Nyx lance plus de lames gravité.|ニクスの重力刃が増える。
upgrade.nyx.singularity.title|Singularity Seed|奇点种子|Семя сингулярности|Semente Singular|Singularitätskeim|Tekillik Tohumu|Graine singulière|特異点の種
upgrade.nyx.singularity.body|Gravity Snare area becomes larger and its center deals more damage.|重力陷阱范围更大，中心伤害更高。|Ловушка шире, центр бьет сильнее.|Armadilha maior e centro mais forte.|Falle größer, Zentrum stärker.|Tuzak büyür, merkezi daha sert vurur.|Piège plus large, centre plus fort.|重力罠が広がり中心火力増。
upgrade.nyx.horizon.title|Event Horizon|事件视界|Горизонт событий|Horizonte de Evento|Ereignishorizont|Olay Ufku|Horizon d’événement|事象の地平
upgrade.nyx.horizon.body|Enemies pulled by Gravity Snare move slower and overheat sooner.|被重力陷阱牵引的敌人变慢，并更快过热。|Враги в ловушке медленнее и быстрее перегреваются.|Inimigos puxados ficam lentos e superaquecem antes.|Gefangene Feinde sind langsamer und überhitzen früher.|Tuzağa çekilen düşman yavaşlar ve erken aşırı ısınır.|Ennemis piégés ralentis et surchauffent plus tôt.|重力罠の敵が遅くなり、早く過熱する。
upgrade.nyx.cantor.title|Gravity Cantor|重力咏唱|Гравикантор|Cantor Grávito|Gravitationskantor|Yerçekimi Kantoru|Cantor gravité|重力詠唱
upgrade.nyx.cantor.body|Using Gravity Snare creates extra orbit blades.|使用重力陷阱时生成额外轨道刃。|Ловушка создает доп. орбитальные клинки.|Usar armadilha cria lâminas orbitais extras.|Falle erzeugt zusätzliche Orbitklingen.|Tuzak ekstra yörünge bıçağı çıkarır.|Utiliser le piège crée des lames orbitales.|重力罠使用時に軌道刃追加。
upgrade.rook.bulwark.title|Bulwark Core|壁垒核心|Ядро бастиона|Núcleo Baluarte|Bollwerkkern|Siper Çekirdeği|Noyau rempart|防壁コア
upgrade.rook.bulwark.body|Rook gains more hull. Shells pierce better.|壁垒生命更高，炮弹穿透更强。|У Рука больше корпуса, снаряды лучше пробивают.|Rook ganha mais casco. Obus perfuram melhor.|Rook bekommt mehr Hülle. Granaten piercen besser.|Rook daha çok gövde alır, mermi daha iyi deler.|Rook gagne plus de coque. Obus perforent mieux.|ルーク耐久増、砲弾貫通強化。
upgrade.rook.siege.title|Siege Battery|攻城电池|Осадная батарея|Bateria de Cerco|Belagerungsbatterie|Kuşatma Bataryası|Batterie de siège|攻城バッテリー
upgrade.rook.siege.body|Rook shells deal more damage and fire a little faster.|壁垒炮弹伤害更高，射速略微提高。|Снаряды Рука сильнее и чуть быстрее.|Obus de Rook causam mais dano e disparam um pouco mais rápido.|Rook-Granaten stärker und etwas schneller.|Rook mermisi güçlü ve biraz hızlı olur.|Obus de Rook plus forts et cadence légère.|ルーク砲弾の火力と射速が少し上がる。
upgrade.rook.aegis.title|Aegis Relay|护盾中继|Реле эгиды|Relé Égide|Aegisrelais|Kalkan Rölesi|Relais égide|イージス中継
upgrade.rook.aegis.body|Bulwark Drive heals more and gives energy.|壁垒推进回复更多生命，并获得能量。|Прорыв лечит больше и дает энергию.|Bulwark cura mais e dá energia.|Bulwark heilt mehr und gibt Energie.|Bulwark daha çok iyileştirir ve enerji verir.|Bulwark soigne plus et donne énergie.|防壁突進の回復増、エネルギー獲得。
upgrade.rook.citadel.title|Citadel Protocol|城垒协议|Протокол цитадели|Protocolo Cidadela|Zitadellenprotokoll|Hisar Protokolü|Protocole citadelle|城塞プロトコル
upgrade.rook.citadel.body|Bulwark Drive blocks a wider area. Shell damage increases.|壁垒推进阻挡范围更宽，炮弹伤害提高。|Прорыв блокирует шире, снаряды сильнее.|Bulwark bloqueia área maior e obus causam mais dano.|Bulwark blockt breiter, Granatenschaden steigt.|Bulwark daha geniş bloklar, mermi hasarı artar.|Bulwark bloque plus large, obus plus forts.|防壁突進の範囲拡大、砲弾火力増。
upgrade.lyra.chord.title|Resonance Chord|共鸣和弦|Резонансный аккорд|Acorde Ressonante|Resonanzakkord|Rezonans Akoru|Accord résonant|共鳴和音
upgrade.lyra.chord.body|Lyra adds extra shot lanes. Every third volley hits harder.|弦歌增加额外弹道。每第三次射击伤害更高。|Лира получает доп. линии. Каждый третий залп сильнее.|Lyra ganha linhas extras. Cada terceira salva bate mais.|Lyra erhält Extra-Bahnen. Jede dritte Salve trifft härter.|Lyra ekstra hat alır. Her üçüncü salvo daha sert vurur.|Lyra ajoute des lignes. Chaque troisième salve frappe plus.|ライラの弾道追加、3回目の斉射強化。
upgrade.lyra.tempo.title|Tempo Bloom|节拍绽放|Расцвет темпа|Florescer Tempo|Tempoblüte|Tempo Çiçeği|Floraison tempo|テンポ開花
upgrade.lyra.tempo.body|Lyra fires faster and gains extra echo shots.|弦歌射速更快，并追加回声弹。|Лира стреляет быстрее и получает эхо-выстрелы.|Lyra atira mais rápido e ganha ecos.|Lyra feuert schneller und erhält Echo-Schüsse.|Lyra hızlı ateş eder, yankı atışı kazanır.|Lyra tire plus vite et gagne des échos.|ライラの射速増、エコー弾追加。
upgrade.lyra.cascade.title|Harmonic Cascade|谐波连瀑|Гармонический каскад|Cascata Harmônica|Harmoniekaskade|Harmonik Çağlayan|Cascade harmonique|調和連瀑
upgrade.lyra.cascade.body|Pulse rings can chain damage and create split shots.|脉冲环可以连锁伤害，并生成分裂弹。|Кольца могут цеплять урон и создавать осколки.|Anéis podem encadear dano e criar divisões.|Ringe können Schaden ketten und Splitter erzeugen.|Halkalar hasarı zincirler ve bölünme çıkarır.|Les anneaux peuvent chaîner et créer des éclats.|リングが連鎖ダメージと分裂弾を作る。
upgrade.lyra.encore.title|Encore Field|返场力场|Поле биса|Campo Encore|Encorefeld|Encore Alanı|Champ rappel|アンコール場
upgrade.lyra.encore.body|Skill lasts longer and gives energy back.|技能持续更久，并返还能量。|Навык длится дольше и возвращает энергию.|Habilidade dura mais e devolve energia.|Skill dauert länger und gibt Energie zurück.|Yetenek uzar ve enerji döndürür.|La compétence dure plus longtemps et rend énergie.|スキル延長、エネルギー返還。
upgrade.orion.spear.title|Comet Spear|彗星星矛|Копье кометы|Lança Cometa|Kometenspeer|Kuyruklu Mızrak|Lance comète|彗星槍
upgrade.orion.spear.body|Main spear damage greatly increases. Boss guard breaks faster.|主星矛伤害大幅提高，更快击破 Boss 护盾。|Главное копье намного сильнее. Защита босса ломается быстрее.|Lança principal causa muito mais dano. Guarda de chefe quebra rápido.|Hauptspeer viel stärker. Boss-Wache bricht schneller.|Ana mızrak çok güçlenir. Boss koruması hızlı kırılır.|Lance principale bien plus forte. Garde de boss brisée plus vite.|主槍火力大幅増、Boss防御を早く割る。
upgrade.orion.deadeye.title|Deadeye Mark|死眼刻痕|Метка снайпера|Marca Certeira|Todesblick-Marke|Keskin Göz İşareti|Marque de tireur|死眼マーク
upgrade.orion.deadeye.body|Marked enemies and low-health enemies take more damage.|被标记敌人和低血敌人受到更多伤害。|Меченые и слабые враги получают больше урона.|Inimigos marcados ou fracos sofrem mais dano.|Markierte oder schwache Feinde nehmen mehr Schaden.|İşaretli veya düşük canlı düşman daha çok hasar alır.|Cibles marquées ou faibles subissent plus.|標的と低耐久の敵に追加ダメージ。
upgrade.orion.quiver.title|Starfall Quiver|星陨箭匣|Колчан звездопада|Aljava Estelar|Sternfallköcher|Yıldız Okluğu|Carquois stellaire|星落の矢筒
upgrade.orion.quiver.body|Orion fires extra side spears.|猎户发射额外侧向星矛。|Орион пускает доп. боковые копья.|Orion dispara lanças laterais extras.|Orion feuert zusätzliche Seitenspeere.|Orion ekstra yan mızrak atar.|Orion tire des lances latérales en plus.|オリオンが側面槍を追加発射。
upgrade.orion.perihelion.title|Perihelion Vector|近日点矢量|Вектор перигелия|Vetor Periélio|Perihelvektor|Günberi Vektörü|Vecteur périhélie|近日点ベクトル
upgrade.orion.perihelion.body|After using skill, dash cooldown resets and energy returns.|使用技能后，冲刺冷却重置并返还能量。|После навыка рывок сбрасывается и энергия возвращается.|Após habilidade, dash reseta e energia volta.|Nach Skill setzt Sprint-CD zurück und Energie kommt zurück.|Yetenek sonrası atılma yenilenir ve enerji döner.|Après compétence, dash réinitialisé et énergie rendue.|スキル後にダッシュCDリセット、エネルギー返還。
upgrade.astra.orbit.title|Prism Orbit|棱镜环绕|Орбита призмы|Órbita Prisma|Prismenorbit|Prizma Yörüngesi|Orbite prisme|プリズム周回
upgrade.astra.orbit.body|Shots gain orbiting prism bolts. Kills split into small shards.|射击追加环绕棱镜弹。击杀会分裂小碎片。|Выстрелы получают призменные болты. Убийства дают осколки.|Tiros ganham prismas orbitais. Abates criam estilhaços.|Schüsse erhalten Prismenkugeln. Kills erzeugen Splitter.|Atışa yörünge prizma eklenir. Öldürmeler parçalar doğurur.|Les tirs gagnent des prismes. Les tués créent des éclats.|周回プリズム弾追加。撃破で小片発生。
upgrade.vesper.overcharge.title|Overcharge Rail|过载轨炮|Перезаряд рельсы|Trilho Sobrecarregado|Überladeschiene|Aşırı Ray|Rail surchargé|過充電レール
upgrade.vesper.overcharge.body|Every few rail shots fire a stronger piercing beam.|每隔几次轨炮，会发射更强的穿透光束。|Каждые несколько рельсов дают сильный луч.|Alguns tiros depois sai um feixe perfurante.|Alle paar Schüsse feuert ein starker Strahl.|Birkaç atışta bir güçlü ışın çıkar.|Tous les quelques tirs, rayon perçant.|数発ごとに強い貫通ビーム。
upgrade.kairo.hunter.title|Hunter Wing|猎手机翼|Крыло охотников|Asa Caçadora|Jägerflügel|Avcı Kanat|Aile chasseuse|ハンター翼
upgrade.kairo.hunter.body|Drones fire homing missiles from their own positions.|无人机会从自身位置发射跟踪导弹。|Дроны стреляют самонаводящимися ракетами.|Drones disparam mísseis guiados.|Drohnen feuern Zielsuchraketen.|Dronlar güdümlü füze atar.|Les drones tirent des missiles guidés.|ドローンが誘導ミサイル発射。
upgrade.sol.ignition.title|Ignition Wave|点燃波|Волна зажигания|Onda de Ignição|Zündwelle|Tutuşma Dalgası|Vague d’allumage|点火波
upgrade.sol.ignition.body|Timed fire waves and kills burn nearby enemies.|定时释放火波，击杀会灼烧附近敌人。|Волны огня и убийства жгут врагов рядом.|Ondas de fogo e abates queimam perto.|Feuerwellen und Kills verbrennen Nähe.|Ateş dalgası ve öldürme yakar.|Vagues de feu, tués brûlent autour.|火波と撃破で周囲炎上。
upgrade.nyx.tax.title|Void Tax|虚空税|Налог пустоты|Taxa do Vazio|Leerenzoll|Boşluk Vergisi|Taxe du vide|虚空税
upgrade.nyx.tax.body|Nearby enemies take more damage and slow down.|靠近你的敌人受到更多伤害，并会减速。|Близкие враги получают больше урона и медлят.|Inimigos próximos sofrem mais e desaceleram.|Nahe Gegner erleiden mehr Schaden und werden langsam.|Yakın düşmanlar fazla hasar alır ve yavaşlar.|Ennemis proches plus touchés et ralentis.|近い敵の被ダメ増、減速。
upgrade.rook.counter.title|Counter Battery|反击炮台|Ответная батарея|Bateria de Contra|Gegenbatterie|Karşı Batarya|Batterie riposte|反撃砲台
upgrade.rook.counter.body|Main shots add heavy side shells. Taking damage fires back.|主炮追加侧向重弹。受到伤害时会反击。|Основной огонь добавляет снаряды. Урон вызывает ответ.|Tiros ganham cascos laterais. Sofrer dano revida.|Hauptfeuer bekommt Seitengeschosse. Treffer kontert.|Ana atış yan mermi alır. Hasar alınca karşılık verir.|Tirs lourds latéraux. Être touché riposte.|側面重弾追加。被弾で反撃。
upgrade.lyra.trigger.title|Beat Trigger|节拍扳机|Такт-триггер|Gatilho de Ritmo|Taktauslöser|Ritim Tetiği|Gâchette tempo|ビートトリガー
upgrade.lyra.trigger.body|Every few beats, Lyra repeats a small echo volley.|每隔几个节拍，弦歌会追加一次回声齐射。|Каждые такты Лира повторяет эхо-залп.|A cada batida, Lyra repete ecos.|Alle paar Takte wiederholt Lyra Echos.|Birkaç ritimde Lyra yankı atar.|Tous les temps, Lyra répète une salve.|数拍ごとに反響斉射。
upgrade.orion.prey.title|Marked Prey|猎物标记|Меченая добыча|Presa Marcada|Markierte Beute|İşaretli Av|Proie marquée|獲物マーク
upgrade.orion.prey.body|Locked targets take extra damage. Low-hull enemies take more.|锁定目标受到额外伤害。残血敌人更痛。|Цели под прицелом получают больше, слабые еще больше.|Alvos travados sofrem mais; feridos sofrem mais.|Erfasste Ziele nehmen Extraschaden, Schwache mehr.|Kilitli hedef ekstra hasar alır, az canlı daha çok.|Cibles verrouillées plus touchées, blessées plus.|ロック対象に追加ダメージ、低耐久ほど増加。
capstone.pulse|MAX RANK: pulse shots gain one extra lane.|满级：脉冲弹额外增加一条弹道。|МАКС: импульсные выстрелы получают доп. линию.|MÁX: tiros pulso ganham uma linha extra.|MAX: Impulsschüsse erhalten eine Zusatzbahn.|MAKS: darbe atışı ekstra hat kazanır.|MAX : tirs pulsés avec une voie en plus.|最大：パルス弾に追加レーン。
capstone.execution|MAX RANK: guard-broken bosses take much more damage.|满级：Boss 破盾后受到大量额外伤害。|МАКС: босс без защиты получает намного больше урона.|MÁX: chefe sem guarda sofre muito mais dano.|MAX: Bosse mit gebrochener Wache nehmen viel mehr Schaden.|MAKS: koruması kırık boss çok daha fazla hasar alır.|MAX : boss à garde brisée subit beaucoup plus.|最大：Boss防御破壊時に大ダメージ。
capstone.stasis|MAX RANK: immediately clear all red bullets on screen.|满级：立即清除屏幕内所有红弹。|МАКС: сразу чистит все красные пули на экране.|MÁX: limpa todos os tiros vermelhos agora.|MAX: räumt sofort alle roten Kugeln auf dem Bildschirm.|MAKS: ekrandaki tüm kırmızı mermileri hemen temizler.|MAX : nettoie immédiatement tous les tirs rouges.|最大：画面内の赤弾を即座に全消去。
capstone.magnet|MAX RANK: pickups pull from very far and trigger more bullet clears.|满级：拾取物吸得更远，并触发更多清弹。|МАКС: сбор тянется очень далеко и чаще чистит пули.|MÁX: coletas vêm de muito longe e limpam mais tiros.|MAX: Pickups ziehen sehr weit und räumen öfter Kugeln.|MAKS: toplama çok uzaktan gelir ve daha çok temizler.|MAX : collectes attirées très loin et nettoyages fréquents.|最大：拾得物の吸引が強化され、弾消し増加。
capstone.ricochet|MAX RANK: ricochet shards can bounce twice.|满级：回弹碎片可以弹跳两次。|МАКС: рикошетные осколки могут прыгнуть дважды.|MÁX: estilhaços ricocheteiam duas vezes.|MAX: Abprall-Splitter können zweimal springen.|MAKS: sekme parçaları iki kez sıçrayabilir.|MAX : les éclats ricochent deux fois.|最大：跳弾破片が2回跳ねる。
capstone.seeker|MAX RANK: fires more missiles and reloads faster.|满级：导弹数量更多，装填更快。|МАКС: больше ракет и быстрее перезарядка.|MÁX: mais mísseis e recarga mais rápida.|MAX: mehr Raketen und schnelleres Nachladen.|MAKS: daha çok füze, daha hızlı dolum.|MAX : plus de missiles, recharge plus rapide.|最大：ミサイル数増加、再装填短縮。
capstone.rebound|MAX RANK: rebound chance rises and clears nearby red bullets.|满级：反弹概率更高，并清除附近红弹。|МАКС: шанс отражения выше и чистит пули рядом.|MÁX: mais chance de rebater e limpa tiros perto.|MAX: höhere Abprallchance und räumt nahe Kugeln.|MAKS: sekme şansı artar, yakındaki kırmızıyı temizler.|MAX : renvoi plus fiable et nettoie les tirs proches.|最大：反射率上昇、近くの赤弾を消す。
capstone.shadow|MAX RANK: ghost shots fire from both sides and slightly home.|满级：左右同时生成影子弹，并轻微跟踪。|МАКС: призрачные выстрелы идут с двух сторон и наводятся.|MÁX: tiros fantasma saem dos dois lados e seguem alvo.|MAX: Geisterschüsse von beiden Seiten mit leichter Suche.|MAKS: hayalet atış iki yandan çıkar ve hafif güder.|MAX : tirs fantômes des deux côtés avec guidage léger.|最大：両側から影弾、少し追尾。
capstone.slug|MAX RANK: giant bullets pierce more and reload faster.|满级：巨弹穿透更高，装填更快。|МАКС: большие снаряды пробивают больше и заряжаются быстрее.|MÁX: projéteis gigantes perfuram mais e recarregam antes.|MAX: Riesengeschosse durchdringen mehr und laden schneller.|MAKS: dev mermi daha çok deler ve hızlı dolar.|MAX : grosses balles percent plus et reviennent plus vite.|最大：巨大弾の貫通と再装填が強化。
capstone.pinball|MAX RANK: bouncing shots gain one more bounce.|满级：弹球弹丸额外反弹一次。|МАКС: отскакивающие выстрелы получают еще один отскок.|MÁX: tiros que quicam ganham mais um quique.|MAX: abprallende Schüsse erhalten einen Zusatzabprall.|MAKS: seken atışlar bir sekme daha kazanır.|MAX : tirs rebondissants avec un rebond en plus.|最大：跳ねる弾の反射回数+1。
capstone.gyro|MAX RANK: auto-lock turn speed greatly increases.|满级：自动锁敌转向速度大幅提高。|МАКС: скорость поворота автозахвата сильно выше.|MÁX: giro da trava automática aumenta muito.|MAX: Auto-Lock dreht deutlich schneller.|MAKS: oto kilit dönüşü büyük ölçüde artar.|MAX : rotation du verrouillage fortement accrue.|最大：自動ロック旋回が大きく上昇。
capstone.thrusters|MAX RANK: dash refreshes once and movement rises.|满级：冲刺立即刷新一次，机动大幅提升。|МАКС: рывок сразу обновляется, подвижность выше.|MÁX: dash recarrega uma vez e mobilidade sobe.|MAX: Dash wird einmal aufgefrischt, Mobilität steigt.|MAKS: atılma bir kez yenilenir, hareket artar.|MAX : le dash se recharge une fois, mobilité accrue.|最大：ダッシュが一度即回復、機動力上昇。
capstone.nyx.orbit|MAX RANK: gravity blades form two stable orbits.|满级：重力刃形成两圈稳定轨道。|МАКС: клинки образуют две устойчивые орбиты.|MÁX: lâminas formam duas órbitas estáveis.|MAX: Klingen bilden zwei stabile Orbits.|MAKS: bıçaklar iki sabit yörünge kurar.|MAX : lames en deux orbites stables.|最大：重力刃が二重軌道になる。
capstone.nyx.singularity|MAX RANK: Gravity Snare pulls harder and weapon fire recovers faster.|满级：重力陷阱牵引更强，武器射击恢复更快。|МАКС: ловушка тянет сильнее, оружие быстрее готово.|MÁX: armadilha puxa mais e arma volta rápido.|MAX: Falle zieht stärker, Waffe erholt schneller.|MAKS: tuzak daha sert çeker, silah hızlı toparlar.|MAX : piège attire plus fort et l’arme récupère vite.|最大：重力罠の引力強化、武器回復高速化。
capstone.nyx.horizon|MAX RANK: trapped enemies overheat and take bonus damage.|满级：被困敌人更快过热，并受到额外伤害。|МАКС: пойманные враги перегреваются и получают бонусный урон.|MÁX: inimigos presos superaquecem e sofrem dano extra.|MAX: Gefangene Feinde überhitzen und nehmen Bonusschaden.|MAKS: yakalanan düşman aşırı ısınır ve bonus hasar alır.|MAX : ennemis piégés surchauffent et subissent plus.|最大：捕らえた敵が過熱し追加ダメージ。
capstone.nyx.cantor|MAX RANK: orbit blades can trigger chain damage.|满级：轨道刃可以触发连锁伤害。|МАКС: орбитальные клинки запускают цепной урон.|MÁX: lâminas orbitais ativam dano em cadeia.|MAX: Orbitklingen lösen Kettenschaden aus.|MAKS: yörünge bıçakları zincir hasarı başlatır.|MAX : lames orbitales déclenchent dégâts en chaîne.|最大：軌道刃が連鎖ダメージを起こす。
capstone.rook.bulwark|MAX RANK: Rook gains much more hull and safer skill casts.|满级：壁垒获得更多生命，技能更安全。|МАКС: у Рука больше корпуса и безопаснее навык.|MÁX: Rook ganha mais casco e habilidade segura.|MAX: Rook erhält viel Hülle und sicheren Skill.|MAKS: Rook çok gövde ve güvenli yetenek alır.|MAX : Rook gagne plus de coque et compétence sûre.|最大：ルーク耐久増、スキル安全性向上。
capstone.rook.siege|MAX RANK: siege shells become the main damage source.|满级：攻城弹成为主要输出。|МАКС: осадные снаряды становятся главным уроном.|MÁX: obus viram a principal fonte de dano.|MAX: Belagerungsgranaten werden Hauptschaden.|MAKS: kuşatma mermisi ana hasar olur.|MAX : les obus deviennent les dégâts principaux.|最大：攻城弾が主火力になる。
capstone.rook.aegis|MAX RANK: blocking bullets heals more hull.|满级：挡下红弹会回复更多生命。|МАКС: блок пуль лечит больше корпуса.|MÁX: bloquear tiros cura mais casco.|MAX: Kugelblock heilt mehr Hülle.|MAKS: mermi bloklamak daha çok iyileştirir.|MAX : bloquer les tirs soigne plus.|最大：弾を防ぐと回復量増加。
capstone.rook.citadel|MAX RANK: shield area widens and shell damage rises.|满级：护盾范围更宽，炮弹伤害更高。|МАКС: щит шире, снаряды сильнее.|MÁX: escudo maior e obus mais fortes.|MAX: Schildbereich breiter, Granaten stärker.|MAKS: kalkan alanı genişler, mermi hasarı artar.|MAX : bouclier plus large, obus plus forts.|最大：盾範囲拡大、砲弾火力増。
capstone.lyra.chord|MAX RANK: every third volley becomes a wide burst.|满级：每第三次射击变成宽幅爆发。|МАКС: каждый третий залп становится широким взрывом.|MÁX: cada terceira salva vira explosão ampla.|MAX: jede dritte Salve wird ein breiter Ausbruch.|MAKS: her üçüncü salvo geniş patlama olur.|MAX : chaque troisième salve explose large.|最大：3回目の斉射が広範囲爆発。
capstone.lyra.tempo|MAX RANK: Lyra gains a defensive echo shot.|满级：弦歌获得护身回声弹。|МАКС: Лира получает защитный эхо-выстрел.|MÁX: Lyra ganha tiro eco defensivo.|MAX: Lyra erhält defensiven Echo-Schuss.|MAKS: Lyra savunma yankı atışı alır.|MAX : Lyra gagne un tir écho défensif.|最大：ライラが防御エコー弾を得る。
capstone.lyra.cascade|MAX RANK: pulse rings trigger chain and split damage together.|满级：脉冲环同时触发连锁和分裂伤害。|МАКС: кольца дают цепной и осколочный урон вместе.|MÁX: anéis ativam cadeia e divisão juntos.|MAX: Ringe lösen Ketten- und Splitterschaden aus.|MAKS: halkalar zincir ve bölünme hasarını birlikte verir.|MAX : anneaux déclenchent chaîne et éclats ensemble.|最大：リングが連鎖と分裂を同時発動。
capstone.lyra.encore|MAX RANK: skill lasts longer and creates more echo shots.|满级：技能持续更久，并生成更多回声弹。|МАКС: навык дольше и дает больше эхо-выстрелов.|MÁX: habilidade dura mais e cria mais ecos.|MAX: Skill dauert länger und erzeugt mehr Echo-Schüsse.|MAKS: yetenek uzar ve daha çok yankı atışı çıkarır.|MAX : compétence plus longue et plus d’échos.|最大：スキル延長、エコー弾増加。
capstone.orion.spear|MAX RANK: spears break Boss guard much faster.|满级：星矛更快击破 Boss 护盾。|МАКС: копья намного быстрее ломают защиту босса.|MÁX: lanças quebram guarda de chefe muito mais rápido.|MAX: Speere brechen Boss-Wache viel schneller.|MAKS: mızrak boss korumasını çok hızlı kırar.|MAX : les lances brisent la garde du boss plus vite.|最大：槍がBoss防御をより速く割る。
capstone.orion.deadeye|MAX RANK: marked targets trigger chain damage.|满级：被标记目标会触发连锁伤害。|МАКС: метки запускают цепной урон.|MÁX: alvos marcados ativam dano em cadeia.|MAX: Markierte Ziele lösen Kettenschaden aus.|MAKS: işaretli hedef zincir hasarı başlatır.|MAX : cibles marquées déclenchent dégâts en chaîne.|最大：標的が連鎖ダメージを起こす。
capstone.orion.quiver|MAX RANK: extra spears reset dash cooldown and return energy.|满级：额外星矛重置冲刺冷却并返还能量。|МАКС: доп. копья сбрасывают рывок и дают энергию.|MÁX: lanças extras resetam dash e devolvem energia.|MAX: Extraspeere setzen Sprint-CD zurück und geben Energie.|MAKS: ekstra mızrak atılmayı yeniler ve enerji verir.|MAX : lances extra réinitialisent le dash et rendent énergie.|最大：追加槍でダッシュCDリセット、エネルギー返還。
capstone.orion.perihelion|MAX RANK: after marking, dash resets and energy returns.|满级：标记后重置冲刺，并返还能量。|МАКС: после метки рывок сбрасывается и энергия возвращается.|MÁX: após marcar, dash reseta e energia volta.|MAX: nach Markierung setzt Sprint zurück und Energie kommt zurück.|MAKS: işaret sonrası atılma yenilenir ve enerji döner.|MAX : après marquage, dash réinitialisé et énergie rendue.|最大：標的付け後にダッシュリセット、エネルギー返還。
capstone.astra.orbit|MAX RANK: prism kills split into more shards and echo chance rises.|满级：棱镜击杀分裂更多碎片，额外射击概率提高。|МАКС: убийства дают больше осколков и эхо чаще.|MÁX: abates geram mais estilhaços e ecos.|MAX: Kills geben mehr Splitter und mehr Echos.|MAKS: öldürme daha çok parça ve yankı verir.|MAX : plus d’éclats et d’échos aux tués.|最大：撃破小片増加、エコー率増加。
capstone.vesper.overcharge|MAX RANK: overcharge fires more often and critical damage rises.|满级：过载更频繁，暴击伤害提高。|МАКС: перезаряд чаще, крит сильнее.|MÁX: sobrecarga mais frequente e crítico maior.|MAX: Überladung öfter, Krit stärker.|MAKS: aşırı atış sıklaşır, kritik artar.|MAX : surcharge plus fréquente, crit accru.|最大：過充電頻度増、クリティカル増。
capstone.kairo.hunter|MAX RANK: hunter missiles refresh immediately and drones shoot sooner.|满级：猎手导弹立即刷新，无人机更快开火。|МАКС: ракеты обновляются, дроны стреляют быстрее.|MÁX: mísseis recarregam e drones atiram cedo.|MAX: Raketen frisch, Drohnen feuern früher.|MAKS: füzeler yenilenir, dron hızlı ateşler.|MAX : missiles prêts, drones tirent plus tôt.|最大：ミサイル即更新、ドローン加速。
capstone.sol.ignition|MAX RANK: ignition waves strengthen chain damage.|满级：点燃波会强化连锁伤害。|МАКС: волны усиливают цепной урон.|MÁX: ignição fortalece cadeias.|MAX: Zündwellen stärken Ketten.|MAKS: tutuşma zinciri güçlendirir.|MAX : vagues renforcent la chaîne.|最大：点火波が連鎖強化。
capstone.nyx.tax|MAX RANK: nearby enemies slow more and global slow improves.|满级：近身敌人减速更强，整体减速也提高。|МАКС: близкие враги медлят сильнее.|MÁX: próximos ficam mais lentos.|MAX: nahe Gegner werden langsamer.|MAKS: yakın düşman daha yavaşlar.|MAX : proches plus ralentis.|最大：近い敵の減速強化。
capstone.rook.counter|MAX RANK: taking damage gives a longer counter window.|满级：受击后的反击窗口更长。|МАКС: после урона окно ответа дольше.|MÁX: dano abre janela de contra maior.|MAX: Treffer geben längeres Konterfenster.|MAKS: hasar sonrası karşı pencere uzar.|MAX : fenêtre de riposte prolongée.|最大：被弾後の反撃時間増。
capstone.lyra.trigger|MAX RANK: beat triggers immediately and echo chance rises.|满级：节拍触发更快，额外射击概率提高。|МАКС: такт срабатывает сразу, эхо чаще.|MÁX: ritmo ativa já e ecos sobem.|MAX: Takt löst sofort aus, Echo steigt.|MAKS: ritim hemen tetikler, yankı artar.|MAX : tempo déclenche vite, écho accru.|最大：即ビート発動、エコー率増。
capstone.orion.prey|MAX RANK: marked prey takes higher critical damage.|满级：猎物标记造成更高暴击伤害。|МАКС: добыча получает больше критурона.|MÁX: presa marcada sofre crítico maior.|MAX: Markierte Beute nimmt mehr Krit.|MAKS: işaretli av daha çok kritik alır.|MAX : proie marquée subit plus de crit.|最大：獲物マークのクリティカル増。
""";

    private static Dictionary<string, MultiLocalizedText> BuildLocalizedOverrides()
    {
        Dictionary<string, MultiLocalizedText> rows = new();
        foreach (string rawLine in LocalizationRows.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("key|", StringComparison.Ordinal))
            {
                continue;
            }

            string[] parts = line.Split('|');
            if (parts.Length != 9)
            {
                continue;
            }

            rows[parts[0]] = new MultiLocalizedText(parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7], parts[8]);
        }

        return rows;
    }

    private static string LanguageDisplayName(GameLanguage language)
    {
        return language switch
        {
            GameLanguage.Chinese => "中文",
            GameLanguage.Russian => "РУССКИЙ",
            GameLanguage.PortugueseBrazil => "PORTUGUÊS BR",
            GameLanguage.German => "DEUTSCH",
            GameLanguage.Turkish => "TÜRKÇE",
            GameLanguage.French => "FRANÇAIS",
            GameLanguage.Japanese => "日本語",
            _ => "ENGLISH",
        };
    }

    private static int LanguageCycleIndex(GameLanguage language)
    {
        for (int i = 0; i < LanguageCycle.Length; i++)
        {
            if (LanguageCycle[i] == language)
            {
                return i;
            }
        }

        return 0;
    }

    private string LocalizeKey(string key, LocalizedText baseText)
    {
        if (LocalizedOverrides.TryGetValue(key, out MultiLocalizedText localized))
        {
            return localized.For(_language);
        }

        if (_language == GameLanguage.English || _language == GameLanguage.Chinese)
        {
            return baseText.ForBaseLanguage(_language);
        }

        return GeneratedLocalizedText(key, baseText.English);
    }

    private string LocalizeMissingKey(string key)
    {
        if (LocalizedOverrides.TryGetValue(key, out MultiLocalizedText localized))
        {
            return localized.For(_language);
        }

        return key;
    }

    private string GeneratedLocalizedText(string key, string english)
    {
        if (key.StartsWith("capstone.", StringComparison.Ordinal))
        {
            return GeneratedCapstoneText(key);
        }

        if (key.EndsWith(".body", StringComparison.Ordinal) || key.EndsWith(".trait", StringComparison.Ordinal) || key.EndsWith(".role", StringComparison.Ordinal))
        {
            return GeneratedEffectText(english);
        }

        return GeneratedShortText(key, english);
    }

    private string GeneratedCapstoneText(string key)
    {
        if (key.Contains("chain", StringComparison.Ordinal) || key.Contains("relay", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "МАКС: цепь прыгает дальше и заканчивается осколками.",
                GameLanguage.PortugueseBrazil => "MÁX: cadeias saltam mais longe e terminam em estilhaços.",
                GameLanguage.German => "MAX: Ketten springen weiter und enden in Splittern.",
                GameLanguage.Turkish => "MAKS: zincir uzağa sıçrar ve parça patlatır.",
                GameLanguage.French => "MAX : la chaîne saute plus loin et finit en éclats.",
                GameLanguage.Japanese => "最大：連鎖が遠くへ跳び、最後に破片爆発。",
                _ => "MAX RANK: chains jump farther and end with a shard burst.",
            };
        }

        if (key.Contains("split", StringComparison.Ordinal) || key.Contains("fractal", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "МАКС: осколки делятся второй раз.",
                GameLanguage.PortugueseBrazil => "MÁX: tiros divididos se dividem de novo.",
                GameLanguage.German => "MAX: Splitter teilen sich ein zweites Mal.",
                GameLanguage.Turkish => "MAKS: bölünen atış ikinci kez bölünür.",
                GameLanguage.French => "MAX : les éclats se divisent une seconde fois.",
                GameLanguage.Japanese => "最大：分裂弾がもう一度分裂。",
                _ => "MAX RANK: split shots create a second split.",
            };
        }

        if (key.Contains("astra", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "МАКС: призмы дают больше прямого урона.",
                GameLanguage.PortugueseBrazil => "MÁX: prismas causam mais dano direto.",
                GameLanguage.German => "MAX: Prismen verursachen mehr direkten Schaden.",
                GameLanguage.Turkish => "MAKS: prizmalar daha çok doğrudan hasar verir.",
                GameLanguage.French => "MAX : les prismes infligent plus de dégâts directs.",
                GameLanguage.Japanese => "最大：プリズムの直接ダメージ増加。",
                _ => "MAX RANK: prism shots deal more direct damage.",
            };
        }

        if (key.Contains("vesper", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "МАКС: рельсы оставляют задержанные линии урона.",
                GameLanguage.PortugueseBrazil => "MÁX: trilhos deixam linhas de dano atrasadas.",
                GameLanguage.German => "MAX: Rails lassen verzögerte Schadenslinien.",
                GameLanguage.Turkish => "MAKS: raylar gecikmeli hasar çizgisi bırakır.",
                GameLanguage.French => "MAX : les rails laissent des lignes de dégâts retardées.",
                GameLanguage.Japanese => "最大：レールが遅延ダメージ線を残す。",
                _ => "MAX RANK: rail shots leave delayed damage lines.",
            };
        }

        if (key.Contains("kairo", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "МАКС: дроны наносят больше урона и возвращают энергию.",
                GameLanguage.PortugueseBrazil => "MÁX: drones causam mais dano e devolvem energia.",
                GameLanguage.German => "MAX: Drohnen verursachen mehr Schaden und geben Energie zurück.",
                GameLanguage.Turkish => "MAKS: dron hasarı artar ve enerji döner.",
                GameLanguage.French => "MAX : drones plus forts et énergie rendue.",
                GameLanguage.Japanese => "最大：ドローン火力増、エネルギー返還。",
                _ => "MAX RANK: drones deal more damage and refund energy.",
            };
        }

        return _language switch
        {
            GameLanguage.Russian => "МАКС: навык лечит, чистит больше пуль и дает щит.",
            GameLanguage.PortugueseBrazil => "MÁX: habilidade cura, limpa mais tiros e dá escudo.",
            GameLanguage.German => "MAX: Skill heilt, räumt mehr Kugeln und gibt Schild.",
            GameLanguage.Turkish => "MAKS: yetenek iyileştirir, daha çok temizler ve kalkan verir.",
            GameLanguage.French => "MAX : compétence soigne, nettoie plus et donne bouclier.",
            GameLanguage.Japanese => "最大：スキルが回復、弾消し、シールドを強化。",
            _ => "MAX RANK: skill heals, clears more bullets, and gives a shield.",
        };
    }

    private string GeneratedEffectText(string english)
    {
        string lower = english.ToLowerInvariant();
        if (TextHas(lower, "chain", "jump", "relay"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Усиливает цепную сборку: попадания чаще переходят к соседним целям.",
                GameLanguage.PortugueseBrazil => "Fortalece builds de cadeia: acertos saltam melhor entre alvos.",
                GameLanguage.German => "Stärkt Ketten-Builds: Treffer springen häufiger auf nahe Ziele.",
                GameLanguage.Turkish => "Zincir yapısını güçlendirir: isabetler yakındaki hedeflere sıçrar.",
                GameLanguage.French => "Renforce les builds chaîne : les impacts sautent mieux entre cibles.",
                GameLanguage.Japanese => "連鎖ビルド強化。命中が近くの敵へ跳びやすくなる。",
                _ => english,
            };
        }

        if (TextHas(lower, "split", "shard", "fractal"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Разгоняет раскол: убийства создают осколки для зачистки волны.",
                GameLanguage.PortugueseBrazil => "Acelera a divisão: abates criam estilhaços para limpar a onda.",
                GameLanguage.German => "Treibt Splitter-Builds an: Kills erzeugen Räumgeschosse.",
                GameLanguage.Turkish => "Bölünme akışını güçlendirir: öldürmeler temizlik parçaları doğurur.",
                GameLanguage.French => "Accélère la fragmentation : les tués créent des éclats de nettoyage.",
                GameLanguage.Japanese => "分裂ビルド強化。撃破で掃討用の破片を生む。",
                _ => english,
            };
        }

        if (TextHas(lower, "drone", "swarm"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Усиливает дронов и превращает приказы рою в плотный огонь.",
                GameLanguage.PortugueseBrazil => "Fortalece drones e transforma ordens em fogo de enxame.",
                GameLanguage.German => "Stärkt Drohnen und macht Befehle zu dichtem Schwarmfeuer.",
                GameLanguage.Turkish => "Dronları güçlendirir, komutları yoğun sürü ateşine çevirir.",
                GameLanguage.French => "Renforce les drones et transforme les ordres en feu d’essaim.",
                GameLanguage.Japanese => "ドローン強化。指令が濃い群体射撃になる。",
                _ => english,
            };
        }

        if (TextHas(lower, "ultimate", "nova", "flare", "judgment", "override"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Экстренная очистка дешевле, а навык лучше держит темп боя.",
                GameLanguage.PortugueseBrazil => "A limpeza de emergência fica barata e a habilidade segura o ritmo.",
                GameLanguage.German => "Noträumung wird billiger und der Skill hält den Kampftakt.",
                GameLanguage.Turkish => "Acil temizlik ucuzlar, yetenek savaş temposunu tutar.",
                GameLanguage.French => "Nettoyage urgent moins cher, compétence plus stable en rythme.",
                GameLanguage.Japanese => "緊急弾消しが軽くなり、スキルが戦闘テンポを保つ。",
                _ => english,
            };
        }

        if (TextHas(lower, "dash", "move", "velocity", "faster", "speed"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Маневренность растет: быстрее движение, рывок и выход из опасных линий.",
                GameLanguage.PortugueseBrazil => "A mobilidade sobe: movimento, dash e fuga de linhas ficam melhores.",
                GameLanguage.German => "Mehr Mobilität: Bewegung, Sprint und Fluchtwege werden stärker.",
                GameLanguage.Turkish => "Hareket artar: hız, atılma ve tehlike çıkışı güçlenir.",
                GameLanguage.French => "Mobilité accrue : déplacement, dash et sorties de danger s’améliorent.",
                GameLanguage.Japanese => "機動力強化。移動、ダッシュ、危険線からの脱出が強くなる。",
                _ => english,
            };
        }

        if (TextHas(lower, "repair", "hull", "shield", "invulnerable", "regenerates"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Живучесть растет: корпус, ремонт и защитные окна становятся надежнее.",
                GameLanguage.PortugueseBrazil => "Sobrevivência sobe: casco, reparo e janelas defensivas melhoram.",
                GameLanguage.German => "Mehr Überleben: Hülle, Reparatur und Schutzfenster werden stärker.",
                GameLanguage.Turkish => "Hayatta kalma artar: gövde, onarım ve savunma penceresi güçlenir.",
                GameLanguage.French => "Survie accrue : coque, réparation et fenêtres défensives progressent.",
                GameLanguage.Japanese => "生存力強化。耐久、修理、防護時間が伸びる。",
                _ => english,
            };
        }

        if (TextHas(lower, "damage", "shots", "fire", "beam", "lance", "pierce", "weapon"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Оружие бьет сильнее и открывает более чистые линии атаки.",
                GameLanguage.PortugueseBrazil => "A arma bate mais forte e abre linhas de ataque mais limpas.",
                GameLanguage.German => "Die Waffe trifft härter und öffnet sauberere Angriffslinien.",
                GameLanguage.Turkish => "Silah daha sert vurur ve daha temiz saldırı hattı açar.",
                GameLanguage.French => "L’arme frappe plus fort et ouvre des lignes d’attaque nettes.",
                GameLanguage.Japanese => "武器火力強化。より明確な攻撃ラインを作る。",
                _ => english,
            };
        }

        if (TextHas(lower, "energy", "focus", "skill", "tactical", "cooldown"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Фокус и энергия возвращаются быстрее, а навык чаще решает волну.",
                GameLanguage.PortugueseBrazil => "Foco e energia voltam mais rápido; habilidade decide mais ondas.",
                GameLanguage.German => "Fokus und Energie kehren schneller zurück, Skills entscheiden öfter.",
                GameLanguage.Turkish => "Odak ve enerji hızlı döner, yetenek dalgaları daha sık çözer.",
                GameLanguage.French => "Focus et énergie reviennent plus vite; la compétence décide plus souvent.",
                GameLanguage.Japanese => "集中とエネルギー回収強化。スキルで波を崩しやすい。",
                _ => english,
            };
        }

        if (TextHas(lower, "pickup", "drops", "dust", "reroll", "score"))
        {
            return _language switch
            {
                GameLanguage.Russian => "Экономика растет: больше добычи, выбора и ресурсов для следующего рывка.",
                GameLanguage.PortugueseBrazil => "Economia melhora: mais drops, escolhas e recursos para a próxima janela.",
                GameLanguage.German => "Mehr Ökonomie: Drops, Auswahl und Ressourcen wachsen.",
                GameLanguage.Turkish => "Ekonomi güçlenir: daha çok ganimet, seçim ve kaynak verir.",
                GameLanguage.French => "Économie renforcée : plus de butin, choix et ressources.",
                GameLanguage.Japanese => "経済強化。ドロップ、選択肢、次の展開資源が増える。",
                _ => english,
            };
        }

        return _language switch
        {
            GameLanguage.Russian => "Открывает более сильный боевой рисунок для этой сборки.",
            GameLanguage.PortugueseBrazil => "Abre um padrão de combate mais forte para esta build.",
            GameLanguage.German => "Schaltet ein stärkeres Kampfmuster für diesen Build frei.",
            GameLanguage.Turkish => "Bu yapı için daha güçlü bir savaş düzeni açar.",
            GameLanguage.French => "Débloque un motif de combat plus fort pour ce build.",
            GameLanguage.Japanese => "このビルドの戦闘パターンを強化する。",
            _ => english,
        };
    }

    private string GeneratedShortText(string key, string english)
    {
        if (key.EndsWith(".title", StringComparison.Ordinal) || key.EndsWith(".name", StringComparison.Ordinal) || key.EndsWith(".weapon", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "Боевой узел",
                GameLanguage.PortugueseBrazil => "Módulo de Combate",
                GameLanguage.German => "Kampfmodul",
                GameLanguage.Turkish => "Savaş Modülü",
                GameLanguage.French => "Module de combat",
                GameLanguage.Japanese => "戦闘モジュール",
                _ => english,
            };
        }

        return _language switch
        {
            GameLanguage.Russian => "Сигнал принят",
            GameLanguage.PortugueseBrazil => "Sinal recebido",
            GameLanguage.German => "Signal empfangen",
            GameLanguage.Turkish => "Sinyal alındı",
            GameLanguage.French => "Signal reçu",
            GameLanguage.Japanese => "信号受信",
            _ => english,
        };
    }

    private static bool TextHas(string text, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (text.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private int TitleFontSize()
    {
        return _language switch
        {
            GameLanguage.Chinese => 72,
            GameLanguage.Japanese => 70,
            GameLanguage.PortugueseBrazil or GameLanguage.French or GameLanguage.Turkish => 58,
            GameLanguage.Russian or GameLanguage.German => 60,
            _ => 66,
        };
    }

    private int LocalizedFontSize(string text, int size, float width)
    {
        float scale = _language switch
        {
            GameLanguage.Russian or GameLanguage.PortugueseBrazil or GameLanguage.German or GameLanguage.Turkish or GameLanguage.French => 0.92f,
            GameLanguage.Japanese => 0.96f,
            _ => 1.0f,
        };

        if (width > 0.0f && text.Length > 0)
        {
            float measured = EstimateTextPixelWidth(text, size) * scale;
            if (measured > width * 1.06f)
            {
                scale *= Mathf.Clamp((width * 1.06f) / measured, 0.72f, 1.0f);
            }
        }

        return Math.Max(9, Mathf.RoundToInt(size * scale));
    }
}
