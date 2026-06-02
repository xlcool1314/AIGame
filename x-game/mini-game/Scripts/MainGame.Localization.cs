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
menu.meta|PERMANENT UPGRADES|永久升级|ПОСТОЯННЫЕ УЗЛЫ|UPGRADES PERMANENTES|DAUER-UPGRADES|KALICI GELİŞİMLER|AMÉLIORATIONS PERMANENTES|恒久強化
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
flow.reward.clean|Clean route: next wave reward improves.|无伤航线：下一波收益提高。|Чистый путь: награда следующей волны выше.|Rota limpa: próxima recompensa melhora.|Saubere Route: nächste Belohnung steigt.|Temiz rota: sonraki ödül artar.|Route propre : prochaine récompense accrue.|無傷航路：次波報酬上昇。
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
hud.controls|WASD/LS MOVE  MOUSE/RS AIM  AUTO FIRE  A/LB DASH  X/RB SKILL  Y/RT ULT  START MENU|WASD/左摇杆移动  鼠标/右摇杆瞄准  自动开火  A/LB冲刺  X/RB技能  Y/RT大招  Start菜单|WASD/LS ДВИЖ.  МЫШЬ/RS ПРИЦЕЛ  АВТООГОНЬ  A/LB РЫВОК  X/RB НАВЫК  Y/RT УЛЬТ|WASD/LS MOVER  MOUSE/RS MIRAR  AUTO  A/LB DASH  X/RB HABIL.  Y/RT ULT|WASD/LS BEWEGEN  MAUS/RS ZIELEN  AUTOFEUER  A/LB SPRINT  X/RB SKILL  Y/RT ULT|WASD/LS HAREKET  FARE/RS NİŞAN  OTO ATEŞ  A/LB ATIL  X/RB YETENEK  Y/RT ULTI|WASD/LS BOUGER  SOURIS/RS VISER  AUTO  A/LB DASH  X/RB COMP.  Y/RT ULT|WASD/LS移動  マウス/RS照準  自動射撃  A/LBダッシュ  X/RBスキル  Y/RT奥義
xp.gain|+{0} EXP|+{0} 经验|+{0} ОПЫТА|+{0} EXP|+{0} EP|+{0} TP|+{0} EXP|+{0} EXP
xp.level_up|LEVEL UP|升级|УРОВЕНЬ +|SUBIU NÍVEL|LEVEL AUF|SEVİYE +|NIVEAU +|レベルアップ
score.cache|SCORE CACHE +{0} DUST|战绩缓存 +{0} 星尘|ЗАПАС +{0} ПЫЛИ|CACHE +{0} PÓ|CACHE +{0} STAUB|ÖDÜL +{0} TOZ|CACHE +{0} POUSSIÈRE|蓄積 +{0} 星屑
score.combo_break|COMBO BREAK|连击中断|КОМБО СБИТО|COMBO QUEBRADO|KOMBO BRICHT|KOMBO BOZULDU|COMBO BRISÉ|コンボ途切れ
title.subtitle|a pilot-build arcade roguelite built in Godot C#|Godot C# 制作的角色构筑弹幕 Roguelite|аркадный roguelite о пилотах и сборках|roguelite arcade de pilotos e builds|Pilot-Build-Arcade-Roguelite in Godot C#|pilot yapı odaklı arcade roguelite|roguelite arcade de pilote et de build|パイロットビルド型ローグライト
title.body|Red fire is always danger. Each pilot has a distinct weapon, pilot skill, and build route. Ultimates are emergency bullet clears.|红弹永远危险。每个角色都有独立武器、技能和构筑路线。大招是紧急清弹。|Красный огонь всегда опасен. У пилотов разное оружие, навык и путь. Ульта чистит пули.|Fogo vermelho é perigo. Cada piloto tem arma, habilidade e rota. Ultimate limpa tiros.|Rotes Feuer ist Gefahr. Jeder Pilot hat Waffe, Skill und Build. Ult räumt Kugeln.|Kırmızı ateş tehlikedir. Her pilotun silahı, yeteneği ve yolu farklı. Ulti mermi temizler.|Le rouge est dangereux. Chaque pilote a arme, compétence et build. L’ultime nettoie les tirs.|赤い攻撃は危険。各機に武器、スキル、ビルドがあり、奥義は緊急弾消し。
title.start|ENTER / CLICK / A|ENTER / 点击 / A 开始|ENTER / КЛИК / A|ENTER / CLIQUE / A|ENTER / KLICK / A|ENTER / TIKLA / A|ENTRÉE / CLIC / A|ENTER / クリック / A
title.won_once|Choir Core fractured once. It remembers.|合唱核心已被击碎一次。它记得你。|Ядро Хора уже треснуло. Оно помнит.|O Núcleo já rachou. Ele lembra.|Der Chorkern brach einmal. Er erinnert sich.|Koro Çekirdeği çatladı. Seni hatırlar.|Le Noyau s’est fissuré. Il se souvient.|合唱核は一度砕けた。覚えている。
pilot.astra.name|Astra|星棱|Астра|Astra|Astra|Astra|Astra|アストラ
pilot.astra.body|Balanced prism bolts. Skill focuses a refraction burst.|均衡棱镜弹。技能凝聚折射爆发。|Сбалансированные призмы. Навык фокусирует преломление.|Disparos prisma equilibrados. Habilidade foca refração.|Ausgewogene Prismenbolzen. Skill bündelt Brechung.|Dengeli prizma atışları. Yeteneği kırılım toplar.|Tirs prismatiques équilibrés. La compétence focalise une réfraction.|バランス型プリズム弾。スキルで屈折集中。
pilot.astra.weapon|Prism Bolts|棱镜连射|Призменные болты|Dardos Prisma|Prismenbolzen|Prizma Mermileri|Traits prismatiques|プリズム弾
pilot.vesper.name|Vesper|暮轨|Веспер|Vesper|Vesper|Vesper|Vesper|ヴェスパー
pilot.vesper.body|Slow heavy rail shots. Skill locks a cutting line.|低频重轨炮。技能锁出切割射线。|Медленная тяжелая рельса. Навык фиксирует линию разреза.|Tiros de trilho pesados. Habilidade trava uma linha.|Langsame schwere Schüsse. Skill sperrt eine Schnittlinie.|Yavaş ağır ray atışı. Yeteneği kesen çizgi kilitler.|Tirs lourds lents. La compétence verrouille une ligne.|重いレール砲。スキルで切断線を固定。
pilot.vesper.weapon|Rail Lance|轨道长枪|Рельсовое копье|Lança de Trilho|Schienenlanze|Ray Mızrağı|Lance-rail|レールランス
pilot.kairo.name|Kairo|环序|Кайро|Kairo|Kairo|Kairo|Kairo|カイロ
pilot.kairo.body|Starts with drones. Skill orders synchronized swarm fire.|开局拥有无人机。技能命令蜂群同步集火。|Начинает с дронами. Навык синхронизирует рой.|Começa com drones. Habilidade sincroniza enxame.|Startet mit Drohnen. Skill befiehlt Schwarmfeuer.|Dronlarla başlar. Yeteneği sürüyü senkronlar.|Commence avec des drones. La compétence synchronise l’essaim.|ドローン持ち。スキルで群れを同期射撃。
pilot.kairo.weapon|Drone Net|无人机网|Сеть дронов|Rede Drone|Drohnennetz|Dron Ağı|Réseau drone|ドローン網
pilot.sol.name|Sol|日冕|Сол|Sol|Sol|Sol|Sol|ソル
pilot.sol.body|Wide scatter fire. Skill opens a protective corona field.|宽角散射。技能展开护身日冕场。|Широкий разброс. Навык открывает защитную корону.|Dispersão ampla. Habilidade abre uma coroa protetora.|Breites Streufeuer. Skill öffnet eine Schutzkorona.|Geniş saçma ateşi. Yeteneği korona alanı açar.|Dispersion large. La compétence ouvre une couronne défensive.|広範囲散弾。スキルで防護コロナ展開。
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
tactical.astra.tip|PRISM FOCUS: lance through the aim lane|棱镜聚焦：沿瞄准线刺穿敌群|ПРИЗМА: пронзает линию прицела|FOCO PRISMA: perfura pela linha de mira|PRISMENFOKUS: durchschlägt die Ziellinie|PRİZMA: nişan hattını deler|FOCUS PRISME : perce la ligne visée|プリズム集中：照準線を貫く
tactical.vesper.tip|RAIL LOCK: pin a line and expose targets|轨道锁定：固定一条高伤切割线|РЕЛЬС: фиксирует линию и раскрывает цели|TRAVA RAIL: fixa uma linha letal|SCHIENENLOCK: nagelt eine Schadenslinie fest|RAY KİLİDİ: ölüm çizgisi kurar|VERROU RAIL : fixe une ligne mortelle|レールロック：高威力線を固定
tactical.kairo.tip|SWARM ORDER: recall drones for one guard volley|蜂群指令：召回无人机同步护航齐射|РОЙ: зовет дронов в защитный залп|ORDEM: drones voltam em salva guardiã|SCHWARM: Drohnen feuern Schutzsalve|SÜRÜ: dronlar koruma salvosu atar|ESSAIM : drones en salve de garde|群体指令：ドローン防護斉射
tactical.sol.tip|CORONA FIELD: defensive clear and recovery|日冕场：清弹、防护并恢复生命|КОРОНА: защита, очистка и ремонт|CORONA: limpa, protege e cura|KORONA: räumt, schützt und heilt|KORONA: temizler, korur, onarır|CORONA : nettoie, protège et répare|コロナ場：弾消し、防護、回復
tactical.cooldown|SKILL COOLDOWN {0}s|技能冷却 {0} 秒|ОТКАТ НАВЫКА {0}с|RECARGA {0}s|SKILL-COOLDOWN {0}s|YETENEK BEKLEME {0}sn|RECHARGE {0}s|スキルCD {0}秒
tactical.focus|FOCUS|专注|ФОКУС|FOCO|FOKUS|ODAK|FOCUS|集中
tactical.clear|CLEAR +{0}|清弹 +{0}|ОЧИСТКА +{0}|LIMPEZA +{0}|RÄUMEN +{0}|TEMİZLE +{0}|NETTOYAGE +{0}|弾消し +{0}
tactical.overheat|OVERHEAT BREAK|过热破绽|ПЕРЕГРЕВ СЛОМАН|QUEBRA DE CALOR|ÜBERHITZUNG BRICHT|AŞIRI ISI KIRILDI|SURCHAUFFE BRISÉE|過熱ブレイク
objective.bonus|BONUS +{0}|奖励 +{0}|БОНУС +{0}|BÔNUS +{0}|BONUS +{0}|BONUS +{0}|BONUS +{0}|ボーナス +{0}
objective.reach_wave|Clear wave {0}|通过第 {0} 波|Зачистить волну {0}|Limpar onda {0}|Welle {0} schaffen|{0}. dalgayı geç|Nettoyer vague {0}|Wave {0} 突破
objective.perfect_waves|Clear {0} clean waves|无伤清波 {0} 次|Чистых волн: {0}|{0} ondas sem dano|{0} saubere Wellen|{0} temiz dalga|{0} vagues parfaites|無傷クリア {0} 回
objective.defeat_enemies|Defeat {0} enemies|击败敌人 {0} 个|Уничтожить врагов: {0}|Derrotar {0} inimigos|{0} Feinde besiegen|{0} düşman yok et|Tuer {0} ennemis|敵撃破 {0}
objective.absorb_bullets|Graze or clear {0} bullets|擦弹或清弹 {0} 次|Задеть или очистить пули: {0}|Raspar ou limpar {0} tiros|{0} Kugeln streifen oder räumen|{0} mermi sıyır/temizle|Frôler ou nettoyer {0} tirs|グレイズ/弾消し {0}
objective.collect_pickups|Collect {0} drops|拾取掉落 {0} 个|Собрать дроп: {0}|Coletar {0} drops|{0} Drops sammeln|{0} ganimet topla|Ramasser {0} butins|ドロップ回収 {0}
objective.best_combo|Reach {0} combo|最高连击 {0}|Комбо {0}|Combo {0}|Kombo {0}|{0} kombo yap|Atteindre combo {0}|コンボ {0}
objective.defeat_bosses|Defeat {0} bosses|击败 Boss {0} 个|Победить боссов: {0}|Derrotar {0} chefes|{0} Bosse besiegen|{0} boss yen|Vaincre {0} boss|ボス撃破 {0}
objective.default|Complete expedition goal|完成远征目标|Выполнить цель|Completar meta|Ziel erfüllen|Hedefi tamamla|Terminer l’objectif|遠征目標達成
objective.complete|GOAL COMPLETE +{0} DUST|目标完成 +{0} 星尘|ЦЕЛЬ +{0} ПЫЛИ|META +{0} PÓ|ZIEL +{0} STAUB|HEDEF +{0} TOZ|OBJECTIF +{0} POUSSIÈRE|目標達成 +{0} 星屑
objective.clean_wave|CLEAN WAVE +ENERGY|无伤清波 +能量|ЧИСТАЯ ВОЛНА +ЭНЕРГИЯ|ONDA LIMPA +ENERGIA|SAUBERE WELLE +ENERGIE|TEMİZ DALGA +ENERJİ|VAGUE PARFAITE +ÉNERGIE|無傷WAVE +エネルギー
next.boss|NEXT: {0}|下一波：{0}|ДАЛЬШЕ: {0}|PRÓXIMA: {0}|NÄCHSTE: {0}|SIRADA: {0}|SUIVANT : {0}|次：{0}
next.primary|NEXT: {0}  PRIMARY: {1}|下一波：{0}  主敌：{1}|ДАЛЬШЕ: {0}  ЦЕЛЬ: {1}|PRÓXIMA: {0}  ALVO: {1}|NÄCHSTE: {0}  PRIMÄR: {1}|SIRADA: {0}  ANA: {1}|SUIVANT : {0}  CIBLE : {1}|次：{0}  主目標：{1}
next.primary_support|NEXT: {0}  PRIMARY: {1}    SUPPORT: {2}|下一波：{0}  主敌：{1}    辅助：{2}|ДАЛЬШЕ: {0}  ЦЕЛЬ: {1}  ПОДДЕРЖКА: {2}|PRÓXIMA: {0}  ALVO: {1}  APOIO: {2}|NÄCHSTE: {0}  PRIMÄR: {1}  SUPPORT: {2}|SIRADA: {0}  ANA: {1}  DESTEK: {2}|SUIVANT : {0}  CIBLE : {1}  SOUTIEN : {2}|次：{0}  主目標：{1}  支援：{2}
choice.instant|Instant|即时|Мгновенно|Instantâneo|Sofort|Anlık|Instantané|即時
choice.tactic|Tactic|战术|Тактика|Tática|Taktik|Taktik|Tactique|戦術
choice.risk|Risk|风险|Риск|Risco|Risiko|Risk|Risque|リスク
choice.contract|Contract|契约|Контракт|Contrato|Vertrag|Kontrat|Contrat|契約
choice.meta|Map|星图|Карта|Mapa|Karte|Harita|Carte|星図
choice.capstone|Capstone|质变|Предел|Ápice|Krönung|Doruk|Apogée|奥義化
choice.path.weapon|WEAPON|武器|ОРУЖИЕ|ARMA|WAFFE|SİLAH|ARME|武器
choice.path.defense|SURVIVE|生存|ЗАЩИТА|DEFESA|SCHUTZ|SAVUNMA|DÉFENSE|防御
choice.path.skill|SKILL|技能|НАВЫК|HABIL.|SKILL|YETENEK|COMP.|スキル
choice.path.flow|CHAIN|连锁|СВЯЗЬ|CADEIA|KETTE|ZİNCİR|CHAÎNE|連鎖
choice.path.economy|GROWTH|成长|РОСТ|GANHO|WACHSTUM|BÜYÜME|GAIN|成長
choice.badge.pilot|PILOT KIT|角色专属|ПИЛОТ|PILOTO|PILOT|PİLOT|PILOTE|機体専用
choice.badge.synergy|SYNERGY|协同|СИНЕРГИЯ|SINERGIA|SYNERGIE|SİNERJİ|SYNERGIE|シナジー
choice.badge.momentum|MOMENTUM|流派惯性|ИНЕРЦИЯ|IMPULSO|MOMENTUM|İVME|ÉLAN|流れ
choice.badge.capstone|CAPSTONE|质变|ПРЕДЕЛ|ÁPICE|KRÖNUNG|DORUK|APOGÉE|最終強化
choice.momentum.open|Build direction open|构筑方向未锁定|Направление свободно|Direção aberta|Build offen|Yön açık|Build ouvert|方針自由
choice.momentum.focus|Build leaning: {0}|构筑倾向：{0}|Уклон: {0}|Rumo: {0}|Tendenz: {0}|Yön: {0}|Orientation : {0}|方針：{0}
build.panel.title|BUILD VECTOR|构筑方向|ВЕКТОР БИЛДА|VETOR DE BUILD|BUILD-VEKTOR|YAPI VEKTÖRÜ|VECTEUR BUILD|ビルド方針
build.panel.open|Open draft|自由选择|Свободный выбор|Escolha livre|Freie Wahl|Serbest seçim|Choix libre|自由選択
build.panel.focus|Leaning {0}|偏向 {0}|Уклон {0}|Rumo {0}|Tendenz {0}|Yön {0}|Orientation {0}|方針 {0}
build.breakthrough|{0} Breakthrough {1}|{0}突破 {1}|Прорыв {0} {1}|Ruptura {0} {1}|{0}-Durchbruch {1}|{0} Atılımı {1}|Percée {0} {1}|{0}突破 {1}
build.breakthrough.weapon|Main weapon line gains sharper tempo and kill pressure.|主武器节奏更顺，击杀线更清晰。|Основное оружие получает темп и давление.|Arma principal ganha ritmo e pressão.|Hauptwaffe erhält Tempo und Druck.|Ana silah tempo ve baskı kazanır.|L’arme principale gagne rythme et pression.|主武器のテンポと撃破力が上がる。
build.breakthrough.defense|Survival route adds hull, damage control, and a brief safety window.|生存路线获得生命、减伤和短暂安全窗口。|Защита дает корпус, снижение урона и окно безопасности.|Defesa dá casco, mitigação e janela segura.|Schutz gibt Hülle, Kontrolle und kurzes Sicherungsfenster.|Savunma gövde, azaltma ve güvenli pencere verir.|La défense ajoute coque, réduction et fenêtre sûre.|防御ルートで耐久、軽減、安全時間を得る。
build.breakthrough.skill|Pilot skill route feeds energy and trims emergency cooldowns.|技能路线补充能量，并压低应急冷却。|Навык дает энергию и режет откаты.|Habilidade dá energia e reduz recargas.|Skill gibt Energie und senkt Notfall-Cooldowns.|Yetenek enerji verir, acil beklemeyi azaltır.|La compétence rend énergie et réduit les délais.|スキルルートでエネルギーと緊急CDを改善。
build.breakthrough.flow|Chain route unlocks relay, split, and echo cleanup.|连锁路线强化中继、分裂和回响清场。|Цепь усиливает реле, раскол и эхо-зачистку.|Cadeia reforça relé, divisão e eco.|Kette stärkt Relais, Splitter und Echo.|Zincir röle, bölünme ve yankıyı güçlendirir.|La chaîne renforce relais, fracture et écho.|連鎖ルートで中継、分裂、反響を強化。
build.breakthrough.economy|Growth route pulls more resources and improves the next wave reward.|成长路线吸取更多资源，并提高下一波收益。|Рост тянет ресурсы и улучшает награду волны.|Ganho puxa recursos e melhora a próxima onda.|Wachstum zieht Ressourcen und stärkt die nächste Belohnung.|Büyüme kaynak çeker, sonraki ödülü artırır.|La croissance attire ressources et améliore la vague suivante.|成長ルートで資源回収と次波報酬が上がる。
upgrade.title|CHOOSE A BUILD UPGRADE|选择一个升级|ВЫБЕРИ УЗЕЛ СБОРКИ|ESCOLHA UM UPGRADE|BUILD-UPGRADE WÄHLEN|YAPI GELİŞİMİ SEÇ|CHOISIS UNE AMÉLIORATION|強化を選択
upgrade.hint|1 / 2 / 3 or click. Gamepad: LS/D-Pad choose, A select, X reroll.|按 1 / 2 / 3 或点击。手柄：摇杆/方向键选择，A确认，X重抽。|1/2/3 или клик. Геймпад: LS/D-Pad выбор, A принять, X реролл.|1/2/3 ou clique. Controle: LS/D-Pad escolhe, A pega, X reroll.|1/2/3 oder Klick. Pad: LS/D-Pad wählen, A nehmen, X neu.|1/2/3 veya tıkla. Gamepad: LS/D-Pad seç, A al, X yenile.|1/2/3 ou clic. Manette : LS/D-Pad choisir, A prendre, X relancer.|1/2/3またはクリック。パッド：LS/十字で選択、A決定、X再抽選。
upgrade.reroll|REROLL {0}|重抽 {0}|РЕРОЛЛ {0}|REROLL {0}|NEU {0}|YENİLE {0}|RELANCE {0}|再抽選 {0}
upgrade.rank_change|RANK {0}  >  {1}|等级 {0}  >  {1}|РАНГ {0}  >  {1}|RANK {0}  >  {1}|RANG {0}  >  {1}|RÜTBE {0}  >  {1}|RANG {0}  >  {1}|ランク {0}  >  {1}
upgrade.select_gamepad|A SELECT|A 选择|A ВЫБОР|A ESCOLHER|A WÄHLEN|A SEÇ|A CHOISIR|A 選択
upgrade.select_key|PRESS {0}|按 {0} 选择|ЖМИ {0}|APERTE {0}|TASTE {0}|{0} BAS|TOUCHE {0}|{0} で選択
rank|Rank {0}|等级 {0}|Ранг {0}|Rank {0}|Rang {0}|Rütbe {0}|Rang {0}|ランク {0}
end.victory.title|CORE FRACTURED|核心破裂|ЯДРО РАСКОЛОТО|NÚCLEO ROMPIDO|KERN GEBROCHEN|ÇEKİRDEK KIRILDI|NOYAU FRACTURÉ|コア破砕
end.defeat.title|SIGNAL LOST|信号丢失|СИГНАЛ ПОТЕРЯН|SINAL PERDIDO|SIGNAL VERLOREN|SİNYAL KOPTU|SIGNAL PERDU|信号途絶
end.wave|REACHED WAVE {0}/{1}|抵达波次 {0}/{1}|ВОЛНА {0}/{1}|ONDA {0}/{1}|WELLE {0}/{1}|DALGA {0}/{1}|VAGUE {0}/{1}|到達 {0}/{1}
end.victory.body|The starfield exhales. Your pattern survives.|星海终于松了一口气。你的轨迹留下来了。|Звезды выдыхают. Твой узор выжил.|O campo estelar respira. Seu padrão venceu.|Das Sternfeld atmet aus. Dein Muster bleibt.|Yıldız alanı nefes alır. İzlerin kalır.|Le champ stellaire expire. Ton motif survit.|星空が息を吐く。軌跡は残った。
end.defeat.body|The Choir rewinds the arena. Tune again.|合唱回卷战场。重新调频。|Хор отматывает арену. Настройся снова.|O Coro reinicia a arena. Sintonize de novo.|Der Chor spult zurück. Stimme dich neu.|Koro arenayı sarar. Yeniden ayarla.|Le Chœur rembobine. Réaccorde-toi.|合唱が戦場を巻き戻す。再調律せよ。
end.restart|ENTER / CLICK / A TO RESTART|ENTER / 点击 / A 重新开始|ENTER / КЛИК / A ПОВТОР|ENTER / CLIQUE / A REINICIAR|ENTER / KLICK / A NEU|ENTER / TIKLA / A YENİDEN|ENTRÉE / CLIC / A REJOUER|ENTER / クリック / A 再開
end.reward|STAR DUST +{0}   REACHED WAVE {1}/40|星尘 +{0}   抵达波次 {1}/40|ПЫЛЬ +{0}   ВОЛНА {1}/40|PÓ +{0}   ONDA {1}/40|STAUB +{0}   WELLE {1}/40|TOZ +{0}   DALGA {1}/40|POUSSIÈRE +{0}   VAGUE {1}/40|星屑 +{0}   到達 {1}/40
end.objective_bonus|GOAL BONUS +{0}|目标奖励 +{0}|БОНУС ЦЕЛИ +{0}|BÔNUS META +{0}|ZIELBONUS +{0}|HEDEF BONUSU +{0}|BONUS OBJECTIF +{0}|目標ボーナス +{0}
end.score_bonus|CACHE DUST +{0}|缓存星尘 +{0}|ПЫЛЬ КЭША +{0}|PÓ DE CACHE +{0}|CACHE-STAUB +{0}|ÖNBELLEK TOZU +{0}|POUSSIÈRE CACHE +{0}|蓄積星屑 +{0}
end.clear_time|CLEAR TIME {0}|通关用时 {0}|ВРЕМЯ {0}|TEMPO {0}|ZEIT {0}|SÜRE {0}|TEMPS {0}|クリア時間 {0}
end.clear_record|NEW CLEAR RECORD #{0}  {1}|新的通关记录 第{0}名  {1}|НОВЫЙ РЕКОРД №{0}  {1}|NOVO RECORDE #{0}  {1}|NEUE BESTZEIT #{0}  {1}|YENİ REKOR #{0}  {1}|NOUVEAU RECORD N°{0}  {1}|新記録 #{0}  {1}
end.unlock_pilot|NEW PILOT UNLOCKED: {0}|新角色解锁：{0}|НОВЫЙ ПИЛОТ: {0}|NOVO PILOTO: {0}|NEUER PILOT: {0}|YENİ PİLOT: {0}|NOUVEAU PILOTE : {0}|新パイロット解放：{0}
end.meta_hint|B/Esc returns to title. Y opens Permanent Upgrades.|B/Esc 返回标题。Y 打开永久升级。|B/Esc к титулу. Y открывает узлы.|B/Esc volta. Y abre upgrades.|B/Esc zurück. Y öffnet Upgrades.|B/Esc başlığa. Y kalıcı gelişim.|B/Esc titre. Y ouvre améliorations.|B/Escで戻る。Yで恒久強化。
meta.title|STAR VAULT|星库|ЗВЕЗДНОЕ ХРАНИЛИЩЕ|COFRE ESTELAR|STERNENARCHIV|YILDIZ KASASI|COFFRE STELLAIRE|星庫
meta.subtitle|Permanent upgrades are a long route across many expeditions.|永久升级是一条跨越许多远征的长线目标。|Постоянные узлы ведут через многие вылеты.|Upgrades permanentes constroem muitas runs.|Dauer-Upgrades tragen viele Expeditionen.|Kalıcı gelişimler uzun sefer yoludur.|Les améliorations permanentes guident plusieurs runs.|恒久強化は遠征を越える長期目標。
meta.dust|Star Dust|星尘|Звездная пыль|Pó Estelar|Sternenstaub|Yıldız Tozu|Poussière stellaire|星屑
meta.wallet|STAR DUST {0}|星尘 {0}|ПЫЛЬ {0}|PÓ {0}|STAUB {0}|TOZ {0}|POUSSIÈRE {0}|星屑 {0}
meta.best|BEST WAVE {0}/40   RUNS {2}|最高波次 {0}/40   出航 {2}|ЛУЧШАЯ {0}/40   ВЫЛЕТЫ {2}|MELHOR {0}/40   RUNS {2}|BESTE {0}/40   RUNS {2}|EN İYİ {0}/40   SEFER {2}|MEILLEURE {0}/40   RUNS {2}|最高 {0}/40   遠征 {2}
leader.title|FASTEST CLEARS|最快通关|ЛУЧШЕЕ ВРЕМЯ|MELHORES TEMPOS|BESTZEITEN|EN İYİ SÜRELER|MEILLEURS TEMPS|最速クリア
leader.rank|#{0}|第 {0} 名|№{0}|#{0}|#{0}|#{0}|N° {0}|#{0}
leader.no_record|--:--|--:--|--:--|--:--|--:--|--:--|--:--|--:--
goal.title|NEXT TARGET|下个目标|СЛЕДУЮЩАЯ ЦЕЛЬ|PRÓXIMA META|NÄCHSTES ZIEL|SONRAKİ HEDEF|PROCHAIN BUT|次の目標
goal.unlock_pilot|Play {0} once to unlock {1}|用 {0} 完成一局，解锁 {1}|Сыграй за {0} один вылет и открой {1}|Jogue uma vez com {0} para liberar {1}|Spiele einmal mit {0}, um {1} freizuschalten|{1} için {0} ile bir sefer oyna|Joue une fois avec {0} pour débloquer {1}|{0}で1回出撃して{1}を解放
goal.clear_40|Clear wave 40 and finish a run|通关 40 波，完成远征|Пройди 40 волн и заверши вылет|Passe a onda 40 e conclua a run|Schaffe Welle 40 und beende den Run|40. dalgayı geç ve seferi bitir|Passe la vague 40 et termine le run|40波を突破して遠征完了
goal.beat_record|Beat your best clear time {0}|突破最快通关 {0}|Побей лучшее время {0}|Bata seu melhor tempo {0}|Unterbiete deine Bestzeit {0}|En iyi süreni geç {0}|Bats ton meilleur temps {0}|最速記録 {0} を更新
goal.set_record|Finish a full clear to set a time|完成一次通关，留下最快时间|Заверши полный проход и поставь время|Conclua uma vitória para marcar tempo|Beende einen Sieg und setze eine Zeit|Tam zaferle süre kaydet|Termine une victoire pour poser un temps|一度クリアして記録を残す
meta.open_hint|Press U or click Permanent Upgrades.|按 U 或点击永久升级。|Нажми U или открой узлы.|Pressione U ou clique upgrades.|Drücke U oder klicke Upgrades.|U bas veya gelişime tıkla.|Appuie sur U ou clique améliorations.|Uまたは恒久強化をクリック。
meta.buy_hint|Click or use LS/D-Pad + A to buy. Keys 1-9 buy nodes. B/Esc returns.|点击或 LS/方向键 + A 购买。1-9 快捷购买。B/Esc 返回。|Клик или LS/D-Pad + A купить. 1-9 покупают. B/Esc назад.|Clique ou LS/D-Pad + A. 1-9 compra. B/Esc volta.|Klick oder LS/D-Pad + A. 1-9 kaufen. B/Esc zurück.|Tıkla veya LS/D-Pad + A. 1-9 satın alır. B/Esc geri.|Clic ou LS/D-Pad + A. 1-9 achète. B/Esc retour.|クリックまたはLS/十字+A。1-9購入。B/Esc戻る。
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
settings.delete_warning|Clears Star Dust, permanent upgrades, unlocks, and records.|清空星尘、永久升级、角色解锁与记录。|Сотрет пыль, узлы, пилотов и рекорды.|Apaga pó, upgrades, desbloqueios e recordes.|Löscht Staub, Upgrades, Freischaltungen und Rekorde.|Toz, gelişim, kilit ve kayıtları siler.|Efface poussière, améliorations, déblocages et records.|星屑、強化、解放、記録を消去。
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
guide.subtitle|Core rules are collected here so the battle HUD can stay clean.|核心规则集中在这里，让战斗界面保持清爽。|Главные правила здесь, чтобы HUD был чистым.|Regras centrais ficam aqui para manter HUD limpo.|Kernregeln stehen hier, damit der HUD sauber bleibt.|Ana kurallar burada, savaş ekranı temiz kalır.|Règles ici pour garder le HUD clair.|主要ルールをここに集約しHUDをすっきり。
guide.page_hint|Left or Right switches tabs. Back returns to settings.|左右切换页签。返回回到设置。|Влево или вправо меняет вкладку. Назад к настройкам.|Esquerda ou direita troca abas. Voltar abre ajustes.|Links oder rechts wechselt Tabs. Zurück öffnet Einstellungen.|Sol veya sağ sekme değiştirir. Geri ayarlara döner.|Gauche ou droite change d’onglet. Retour ouvre les paramètres.|左右でタブ切替。戻るで設定へ。
guide.tab.0|BASICS|基础|ОСНОВЫ|BÁSICO|BASIS|TEMEL|BASES|基本
guide.tab.1|COMBO|连击节奏|КОМБО|COMBO|KOMBO|KOMBO|COMBO|コンボ
guide.tab.2|BUILDS|构筑|СБОРКИ|BUILDS|BUILDS|YAPILAR|BUILDS|ビルド
guide.tab.3|PROGRESS|进程|ПРОГРЕСС|PROGRESSO|FORTSCHRITT|İLERLEME|PROGRESSION|進行
guide.basic.0|Move with WASD, arrows, or left stick. Aim with mouse or right stick. Weapons fire automatically.|WASD、方向键或左摇杆移动。鼠标或右摇杆瞄准。武器自动开火。|WASD, стрелки или левый стик для движения. Мышь или правый стик для прицела. Оружие стреляет само.|Mova com WASD, setas ou analógico esquerdo. Mire com mouse ou analógico direito. Armas disparam sozinhas.|WASD, Pfeile oder linker Stick bewegen. Maus oder rechter Stick zielt. Waffen feuern automatisch.|WASD, yön tuşları veya sol çubuk hareket. Fare veya sağ çubuk nişan. Silahlar otomatik ateş eder.|WASD, flèches ou stick gauche pour bouger. Souris ou stick droit pour viser. Tir automatique.|WASD、矢印、左スティックで移動。マウス、右スティックで照準。自動射撃。
guide.basic.1|Enemy bullets are always red and dangerous. Gray blocks are EXP pickups and disappear if ignored.|敌方子弹永远是红色并且危险。灰色方块是经验，太久不吃会消失。|Вражеские пули всегда красные и опасные. Серые блоки это опыт, он исчезает.|Tiros inimigos são sempre vermelhos e perigosos. Blocos cinza são EXP e somem.|Feindkugeln sind immer rot und gefährlich. Graue Blöcke sind EP und verschwinden.|Düşman mermileri kırmızı ve tehlikeli. Gri bloklar TP, beklerse kaybolur.|Les tirs ennemis sont rouges et dangereux. Les blocs gris sont de l’EXP et expirent.|敵弾は常に赤く危険。灰色ブロックはEXPで放置すると消える。
guide.basic.2|SPACE or X/RB uses the pilot skill. Each pilot solves a different problem: line burst, swarm, shield, gravity, rhythm, or marks.|空格或 X/RB 释放角色技能。不同角色处理不同问题：直线爆发、蜂群、护盾、重力、节拍或标记。|Пробел или X/RB активирует навык. Пилоты решают разные задачи: линия, рой, щит, гравитация, ритм или метка.|Espaço ou X/RB usa habilidade. Cada piloto resolve linha, enxame, escudo, gravidade, ritmo ou marcas.|Leertaste oder X/RB nutzt den Skill. Piloten lösen Linie, Schwarm, Schild, Schwerkraft, Rhythmus oder Markierungen.|Space veya X/RB yetenek kullanır. Pilotlar çizgi, sürü, kalkan, yerçekimi, ritim veya işaret çözer.|Espace ou X/RB lance la compétence. Chaque pilote gère ligne, essaim, bouclier, gravité, rythme ou marques.|SpaceまたはX/RBでスキル。直線、群れ、盾、重力、リズム、マーキングに分かれる。
guide.basic.3|F/E or Y/RT spends high energy to clear nearby bullets. It has a cooldown, so save it for crowded moments.|F/E 或 Y/RT 消耗大量能量清除近身子弹。它有冷却，适合留给最乱的时候。|F/E или Y/RT тратит много энергии и чистит пули рядом. Есть откат, береги для хаоса.|F/E ou Y/RT gasta muita energia para limpar tiros perto. Tem recarga, guarde para caos.|F/E oder Y/RT räumt nahe Kugeln für viel Energie. Es hat Abklingzeit, spare es für Druck.|F/E veya Y/RT çok enerjiyle yakın mermi temizler. Bekleme var, kalabalığa sakla.|F/E ou Y/RT dépense beaucoup d’énergie pour nettoyer près de toi. Garde-le pour les moments denses.|F/EまたはY/RTで高エネルギー消費の近距離弾消し。CDがあるので温存。
guide.combo.0|Combo continues until you take damage. Kills, bullet clears, and pickups build the run’s rhythm.|连击会持续到你受伤为止。击杀、清弹和拾取会一起堆起本局节奏。|Комбо держится до урона. Убийства, очистки и сбор задают ритм забега.|Combo dura até você sofrer dano. Abates, limpezas e coletas criam ritmo.|Kombo hält bis du Schaden nimmst. Kills, Räumen und Sammeln bauen Rhythmus.|Hasar alana kadar kombo sürer. Öldürme, temizleme ve toplama ritmi kurar.|Le combo tient jusqu’aux dégâts. Kills, nettoyages et collectes créent le rythme.|被弾までコンボ継続。撃破、弾消し、回収が走りのリズムを作る。
guide.combo.1|Combo adds a small pace bonus. Each wave now ramps by itself: light opening, faster reserves, bigger late batches.|连击只提供少量节奏加成。每波会自己升压：开场较轻，后续刷新更快，后段批量更大。|Комбо дает малый темп. Волна сама растет: легкий старт, быстрее резерв, крупнее финал.|Combo dá bônus leve. Cada onda escala: início leve, reserva rápida, grupos maiores no fim.|Kombo gibt nur etwas Tempo. Jede Welle steigt: leichter Start, schnellere Reserve, größere Endgruppen.|Kombo küçük tempo verir. Her dalga yükselir: hafif başlangıç, hızlı yedek, büyük son gruplar.|Le combo donne un petit bonus. Chaque vague monte : début léger, renforts rapides, gros groupes tardifs.|コンボは小さな速度補正。各波は軽く始まり、後半ほど速く大群になる。
guide.combo.2|Taking damage breaks combo, resets PACE, and gives the battle room to breathe again.|受伤会打断连击、重置节奏，让战场重新慢下来。|Урон сбивает комбо, сбрасывает темп и снова дает передышку.|Tomar dano quebra combo, reinicia ritmo e dá respiro.|Schaden bricht Kombo, setzt Tempo zurück und gibt Luft.|Hasar kombo kırar, tempoyu sıfırlar ve alan açar.|Les dégâts cassent le combo, réinitialisent le rythme et redonnent de l’air.|被弾でコンボと速度が戻り、戦場に余裕が生まれる。
guide.combo.3|Late game becomes a speedrun choice: stay clean to pull danger forward, or reset pace to survive.|后期会变成竞速选择：保持无伤把危险提前拉来，或主动降速求稳。|Поздняя игра это выбор спидрана: играй чисто и тяни опасность вперед или сбрось темп ради выживания.|No fim vira speedrun: jogue limpo para puxar perigo ou reduza ritmo para viver.|Spätspiel wird Speedrun: sauber bleiben und Gefahr vorziehen oder Tempo senken.|Geç oyun hız koşusu olur: temiz kal ve tehlikeyi çek ya da hayatta kalmak için yavaşla.|Fin de run en mode speedrun: rester propre attire le danger, ralentir protège.|終盤はスピードラン判断。無傷で危険を前倒しするか、速度を落として生存。
guide.build.0|Enemies drop gray EXP blocks. A full EXP bar pauses the fight and opens upgrade choices.|敌人掉落灰色经验块。经验条满后暂停战斗并弹出升级选择。|Враги роняют серый опыт. Полная шкала ставит бой на паузу и открывает выбор.|Inimigos soltam EXP cinza. Barra cheia pausa e abre upgrades.|Feinde lassen graue EP fallen. Volle Leiste pausiert und öffnet Upgrades.|Düşmanlar gri TP düşürür. Bar dolunca savaş durur ve seçim açılır.|Les ennemis lâchent de l’EXP grise. Barre pleine met en pause et ouvre un choix.|敵は灰色EXPを落とす。満タンで戦闘停止し強化選択。
guide.build.1|Role cards and public cards appear together. Picking a direction increases the weight of matching future cards.|角色卡和公共卡会混在一起。持续选择某个方向，会提高同类卡后续出现权重。|Карты пилота и общие карты смешаны. Выбор направления повышает шанс похожих карт.|Cartas de piloto e públicas aparecem juntas. Escolhas parecidas ganham peso futuro.|Piloten- und Allgemeinkarten erscheinen zusammen. Gewählte Richtung erhöht spätere Chance.|Rol ve ortak kartlar birlikte gelir. Seçtiğin yön ileride daha sık görünür.|Cartes de rôle et communes se mélangent. Choisir une voie augmente son poids.|固有カードと共通カードが混ざる。選んだ方向の出現率が上がる。
guide.build.2|Maxing a stream unlocks a capstone that changes how the build plays: chain, split, drones, shield, rhythm, or precision.|流派升满会出现质变效果，改变打法：连锁、分裂、无人机、护盾、节拍或精准。|Максимум ветки дает предел и меняет стиль: цепь, раскол, дроны, щит, ритм или точность.|Completar uma linha libera ápice: cadeia, divisão, drones, escudo, ritmo ou precisão.|Maximale Linie gibt Krönung: Kette, Splitter, Drohnen, Schild, Rhythmus oder Präzision.|Bir akışı bitirmek doruk açar: zincir, bölünme, dron, kalkan, ritim veya keskinlik.|Maxer une voie ouvre un apogée: chaîne, fragmentation, drones, bouclier, rythme ou précision.|系統を最大化すると質変。連鎖、分裂、ドローン、盾、リズム、精密。
guide.build.3|Rerolls are limited. Save them for cards that do not fit your pilot, current route, or survival needs.|重抽次数有限。把它留给不适合角色、流派或生存需求的选项。|Рероллы ограничены. Береги их для карт не под пилота, путь или выживание.|Rerolls são limitados. Guarde para cartas ruins para piloto, rota ou defesa.|Neuwürfe sind begrenzt. Spare sie für Karten ohne Pilot-, Build- oder Schutzwert.|Yenileme sınırlı. Pilota, yola veya savunmaya uymayan kartlara sakla.|Relances limitées. Garde-les pour cartes hors pilote, build ou survie.|再抽選は有限。機体、ルート、生存に合わない時に使う。
guide.progress.0|Pilots unlock in a fixed strength route. Play one run with the current pilot to unlock the next pilot.|角色按强度顺序解锁。用当前角色完整玩一局，就会解锁下一个角色。|Пилоты открываются по силе. Сыграй один вылет текущим пилотом, чтобы открыть следующего.|Pilotos desbloqueiam em rota fixa. Jogue uma run com o atual para abrir o próximo.|Piloten öffnen in fester Stärke-Reihe. Ein Run mit aktuellem Pilot öffnet den nächsten.|Pilotlar güç sırasıyla açılır. Mevcut pilotla bir sefer oyna, sıradaki açılır.|Les pilotes se débloquent par ordre de puissance. Une run avec l’actuel ouvre le suivant.|機体は強さ順に解放。現在の機体で1回遊ぶと次が開く。
guide.progress.1|Star Dust buys permanent upgrades. Costs are tuned as a long journey, not something to finish in one run.|星尘用于购买永久升级。价格按长线成长设计，不应该一局买满。|Пыль покупает постоянные узлы. Цены рассчитаны на долгий путь, не на один забег.|Pó Estelar compra upgrades permanentes. Custos são jornada longa, não uma run.|Sternenstaub kauft Dauer-Upgrades. Kosten sind Langzeitreise, nicht ein Run.|Yıldız Tozu kalıcı gelişim alır. Maliyetler tek sefer değil uzun yol içindir.|La poussière achète des améliorations permanentes. Les coûts visent le long terme.|星屑で恒久強化。価格は長期進行向けで1回完了ではない。
guide.progress.2|Wave pace and boss variants come from shared pools, so the route changes between runs.|波次节奏和 Boss 变体来自共享池，每局路线都会有变化。|Темп волн и боссы берутся из пулов, маршрут меняется каждый забег.|Ritmo de ondas e chefes vêm de pools, então a rota muda.|Wellentempo und Bosse kommen aus Pools, daher wechselt die Route.|Dalga ritmi ve bosslar havuzdan gelir, rota değişir.|Rythme de vagues et boss viennent de pools, la route varie.|波テンポとボスはプール抽選で、毎回ルートが変わる。
guide.progress.3|Main goal: clear 40 waves. Advanced goal: keep combo high and finish faster without losing control.|主要目标是通过 40 波。进阶目标是保持高连击，在不失控的情况下更快通关。|Главная цель: 40 волн. Продвинутая: держать комбо и пройти быстрее без хаоса.|Meta principal: 40 ondas. Meta avançada: combo alto e final rápido sem perder controle.|Hauptziel: 40 Wellen. Fortgeschritten: hohe Kombo und schneller Abschluss ohne Kontrollverlust.|Ana hedef 40 dalga. İleri hedef yüksek kombo ve kontrolü kaybetmeden hızlı bitiş.|But principal: 40 vagues. But avancé: combo élevé et fin rapide sans perdre le contrôle.|目標は40波突破。上級目標は高コンボで制御しながら高速クリア。
guide.0|Move with WASD, arrows, or left stick. Aim with mouse or right stick; weapons fire automatically.|WASD / 方向键 / 左摇杆移动。鼠标或右摇杆瞄准，武器自动开火。|WASD, стрелки или левый стик для движения. Мышь или правый стик для прицела; оружие стреляет само.|Mova com WASD, setas ou analógico esquerdo. Mire com mouse ou analógico direito; armas disparam sozinhas.|WASD, Pfeile oder linker Stick bewegen. Maus oder rechter Stick zielt; Waffen feuern automatisch.|WASD, yön tuşları veya sol çubuk hareket. Fare/sağ çubuk nişan; silahlar otomatik ateş eder.|WASD, flèches ou stick gauche. Souris ou stick droit pour viser; tir automatique.|WASD/矢印/左スティックで移動。マウス/右スティックで照準、自動射撃。
guide.1|Enemy bullets are always red, and red fire is always danger. Grazing, clearing, and steady hits build focus.|敌方子弹统一为红色，红色永远危险。擦弹、清弹和稳定命中会积累专注。|Вражеские пули всегда красные и опасны. Задевание, очистка и стабильные попадания копят фокус.|Tiros inimigos são sempre vermelhos e perigosos. Raspar, limpar e acertar gera foco.|Feindkugeln sind immer rot und gefährlich. Streifen, Räumen und Treffer bauen Fokus auf.|Düşman mermileri kırmızı ve tehlikelidir. Sıyırma, temizleme ve isabet odak verir.|Les tirs ennemis sont rouges et dangereux. Frôler, nettoyer et toucher charge le focus.|敵弾は赤で常に危険。グレイズ、弾消し、命中で集中が増える。
guide.2|SPACE or X/RB fires your pilot skill: prism focus, rail lock, swarm order, or corona field.|空格或 X/RB 释放角色技能：棱镜聚焦、轨道锁定、蜂群指令或日冕场。|Пробел или X/RB активирует навык пилота: призма, рельса, рой или корона.|Espaço ou X/RB usa habilidade: prisma, trilho, enxame ou corona.|Leertaste oder X/RB nutzt den Pilotenskill: Prisma, Rail, Schwarm oder Korona.|Space veya X/RB pilot yeteneği: prizma, ray, sürü veya korona.|Espace ou X/RB lance la compétence : prisme, rail, essaim ou corona.|SpaceまたはX/RBでスキル：プリズム、レール、群体、コロナ。
guide.3|Enemies telegraph, fire a volley, then briefly overheat. Overheat is a bonus damage window, not a timing tax.|敌人会先蓄力、齐射，然后短暂过热。过热是额外输出窗口，不再要求切状态。|Враги заряжаются, дают залп и перегреваются. Перегрев — окно урона, не наказание таймингом.|Inimigos carregam, disparam e superaquecem. Superaquecimento é janela de dano.|Feinde laden, feuern und überhitzen. Überhitzung ist dein Schadensfenster.|Düşman yüklenir, salvo atar, aşırı ısınır. Bu hasar fırsatıdır.|Les ennemis chargent, tirent puis surchauffent. C’est une fenêtre de dégâts.|敵は溜め、斉射し、過熱する。過熱は攻撃チャンス。
guide.4|Right mouse, Shift, A, LB, or LT dashes. Dash briefly grants invulnerability and clears nearby bullets.|右键 / Shift / A / LB / LT 冲刺。冲刺短暂无敌并清除近身子弹。|ПКМ, Shift, A, LB или LT — рывок. Он дает неуязвимость и чистит пули рядом.|Botão direito, Shift, A, LB ou LT dá dash. Dash concede invulnerabilidade curta e limpa tiros.|Rechtsklick, Shift, A, LB oder LT sprintet. Sprint macht kurz unverwundbar und räumt Kugeln.|Sağ tık, Shift, A, LB veya LT atıl. Kısa dokunulmazlık ve yakın mermi temizler.|Clic droit, Shift, A, LB ou LT dash. Brève invulnérabilité et nettoyage proche.|右クリック/Shift/A/LB/LTでダッシュ。短時間無敵と近距離弾消し。
guide.5|F/E or Y/RT spends high energy on an emergency bullet clear. It also has a cooldown.|F / E 或 Y/RT 消耗大量能量释放紧急清弹，并且有冷却。|F/E или Y/RT тратит много энергии на очистку пуль. Есть откат.|F/E ou Y/RT gasta muita energia numa limpeza de emergência. Tem recarga.|F/E oder Y/RT nutzt viel Energie für Noträumung. Es hat Abklingzeit.|F/E veya Y/RT çok enerjiyle acil temizlik yapar. Bekleme var.|F/E ou Y/RT dépense beaucoup d’énergie pour un nettoyage urgent. Recharge incluse.|F/EまたはY/RTで高コストの緊急弾消し。CDあり。
guide.6|Expeditions rotate through swarm, elite, supply, pressure, and boss waves. Some clears chain forward and grant focus, energy, and short damage momentum.|远征会在蜂群、精英、补给、压迫和 Boss 波之间轮转。部分连战会奖励专注、能量和短时伤害动量。|Экспедиции сменяют рой, элиту, снабжение, давление и боссов. Быстрая зачистка дает фокус, энергию и темп.|Expedições alternam enxame, elite, suprimento, pressão e chefes. Limpezas encadeadas dão foco, energia e dano.|Expeditionen wechseln Schwarm, Elite, Versorgung, Druck und Boss. Ketten-Clears geben Fokus, Energie und Schaden.|Seferler sürü, elit, ikmal, baskı ve boss dalgalarını döndürür. Zincir temizlik odak, enerji ve hasar verir.|Les runs alternent essaim, élite, ravitaillement, pression et boss. Les enchaînements donnent focus, énergie et dégâts.|遠征は群襲、精鋭、補給、圧力、ボスを巡る。連続突破で集中、エネルギー、短時間火力。
guide.7|Chain builds reward dense groups; split builds reward kill momentum. Both archetypes are seeded by pilot and wave context.|连锁流适合密集敌群，分裂流适合击杀滚雪球。两种流派会随角色与波次逐步出现。|Цепи сильны по толпе, расколы — от темпа убийств. Оба пути зависят от пилота и волн.|Build de cadeia vence grupos; divisão cresce com abates. Ambos surgem por piloto e onda.|Ketten lieben Gruppen; Splitter lieben Kill-Tempo. Beide hängen von Pilot und Welle ab.|Zincir kalabalıkta, bölünme öldürme temposunda güçlenir. İkisi pilot ve dalgaya bağlıdır.|Chaîne aime les groupes; fragmentation aime le rythme. Les deux viennent du pilote et des vagues.|連鎖は密集、分裂は撃破連鎖向け。機体と波で出現しやすい。
guide.8|Enemies drop gray EXP blocks. They expire if ignored, and a full EXP bar immediately opens a three-card upgrade.|敌人掉落灰色经验块。太久不拾取会消失，经验条满后立刻弹出三选一。|Враги роняют серый опыт. Он исчезает, если медлить. Полная шкала открывает три карты.|Inimigos soltam EXP cinza. Expira se ignorado; barra cheia abre três cartas.|Feinde lassen graue EP fallen. Sie verfallen; volle EP-Leiste öffnet drei Karten.|Düşmanlar gri TP düşürür. Toplanmazsa yok olur; dolu bar üç kart açar.|Les ennemis lâchent de l’EXP grise. Elle expire; barre pleine ouvre trois cartes.|敵は灰色EXPを落とす。放置で消え、満タンで三択強化。
tutorial.1|Red fire is always danger. Graze and clear bullets to build focus.|红弹永远危险。擦弹和清弹会积累专注。|Красный огонь опасен. Задевай и чисти пули ради фокуса.|Fogo vermelho é perigo. Raspe e limpe tiros para foco.|Rotes Feuer ist Gefahr. Streifen und Räumen gibt Fokus.|Kırmızı ateş tehlikedir. Sıyır ve temizle, odak kazan.|Le rouge est dangereux. Frôle et nettoie pour charger le focus.|赤弾は危険。グレイズと弾消しで集中。
tutorial.2|Press SPACE or X/RB for your pilot skill. Higher focus makes it stronger.|按空格或 X/RB 释放角色技能。专注越高，效果越强。|Пробел или X/RB — навык. Чем выше фокус, тем сильнее.|Espaço ou X/RB usa habilidade. Mais foco, mais força.|Leertaste oder X/RB nutzt den Skill. Mehr Fokus, stärker.|Space veya X/RB yetenek. Odak arttıkça güçlenir.|Espace ou X/RB : compétence. Plus de focus, plus de puissance.|Space/X/RBでスキル。集中が高いほど強い。
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
meta.hull.body|Start every run with more maximum hull.|每局开局拥有更高生命上限。|Каждый вылет начинается с большим корпусом.|Comece cada run com mais casco máximo.|Starte jeden Run mit mehr Hülle.|Her sefere daha yüksek gövdeyle başla.|Commence chaque run avec plus de coque.|各遠征の最大耐久が増える。
meta.energy.body|Start with more energy and a larger energy cap.|提高开局能量和能量上限。|Больше стартовой энергии и лимита.|Mais energia inicial e limite maior.|Mehr Startenergie und höheres Limit.|Daha fazla başlangıç enerjisi ve kapasite.|Plus d’énergie de départ et de réserve.|開始エネルギーと上限が増える。
meta.weapon.body|Weapon damage starts higher before any draft choices.|每局开局武器伤害更高。|Оружие сильнее еще до выбора сборки.|Armas começam causando mais dano.|Waffen starten mit mehr Schaden.|Silah hasarı baştan yükselir.|Les armes commencent plus fortes.|武器初期ダメージが上がる。
meta.engine.body|Move faster and dash harder from wave one.|从第一波开始移动更快、冲刺更强。|С первой волны быстрее движение и рывок.|Mova mais rápido e dê dash melhor desde a onda um.|Ab Welle eins schneller und härter sprinten.|İlk dalgadan itibaren daha hızlı ve sert atıl.|Plus rapide dès la première vague.|Wave1から移動とダッシュ強化。
meta.salvage.body|Earn more Star Dust and pull pickups from farther away.|星尘收益更高，拾取范围更远。|Больше пыли и дальний сбор.|Mais Pó Estelar e atração maior.|Mehr Staub und größere Sammelreichweite.|Daha çok Toz ve daha uzak toplama.|Plus de poussière et aimant plus large.|星屑獲得と吸引範囲が増える。
meta.chart.body|Gain extra rerolls on every upgrade screen.|每次升级选择获得更多重抽。|Больше рероллов на каждом выборе.|Mais rerolls em cada escolha.|Mehr Neuwürfe bei jeder Wahl.|Her seçimde ekstra yenileme.|Relances supplémentaires à chaque choix.|各強化画面の再抽選が増える。
meta.repair.body|Repair drops become more common and calm moments slowly restore hull.|修复掉落更多，脱战后缓慢回血。|Ремонт падает чаще, в паузах корпус восстанавливается.|Reparos caem mais; momentos calmos curam.|Reparaturen fallen öfter, Ruhe heilt Hülle.|Onarım daha sık düşer, sakin anlar iyileştirir.|Réparations plus fréquentes, le calme répare.|修理ドロップ増加、非戦闘時に回復。
meta.aegis.body|Incoming damage is reduced before temporary shields or repairs.|先降低受到的伤害，再计算护盾与修复。|Урон снижается до щитов и ремонта.|Reduz dano antes de escudos e reparos.|Schaden sinkt vor Schild und Reparatur.|Kalkan/onarım öncesi hasarı azaltır.|Réduit les dégâts avant boucliers et soins.|被ダメージを先に軽減。
meta.nova.body|Emergency clears cost less energy and each run starts with more charge.|紧急清弹耗能降低，开局拥有更多能量。|Очистка дешевле, стартовый заряд выше.|Limpezas custam menos e começam mais cheias.|Noträumung kostet weniger und startet geladener.|Acil temizlik ucuzlar, başlangıç enerjisi artar.|Nettoyage urgent moins cher et départ mieux chargé.|緊急弾消しコスト減、開始チャージ増。
meta.drone.body|Begin runs with support drones. Kairo turns this into a stronger swarm.|开局获得支援无人机。环序会把它强化成蜂群。|Стартовые дроны поддержки. Кайро усиливает рой.|Comece com drones; Kairo vira enxame forte.|Starte mit Drohnen; Kairo macht daraus Schwarm.|Destek dronlarıyla başla; Kairo sürüye çevirir.|Drones de départ; Kairo les transforme en essaim.|支援ドローン開始。カイロは群れへ強化。
meta.tuner.body|Pilot skills cool down faster and focus gains return more energy.|角色技能冷却更快，专注收益带来更多能量。|Навыки остывают быстрее, фокус дает больше энергии.|Habilidades recarregam mais rápido e foco dá energia.|Skills laden schneller, Fokus gibt mehr Energie.|Yetenekler hızlı döner, odak enerji getirir.|Compétences plus rapides, focus rend énergie.|スキルCD短縮、集中でエネルギー増。
meta.survey.body|Earn a modest Star Dust bonus and read the opening waves more safely.|略微提高星尘收益，让开局节奏更稳。|Небольшой бонус пыли и безопасный старт.|Bônus leve de Pó e início mais seguro.|Etwas mehr Staub und sicherer Auftakt.|Az Toz bonusu ve güvenli açılış.|Petit bonus de poussière et ouverture sûre.|星屑少量増加、序盤が安定。
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
upgrade.bounty.title|Bounty Contract|赏金契约|Контракт охоты|Contrato de Recompensa|Kopfgeldvertrag|Ödül Kontratı|Contrat de prime|賞金契約
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
upgrade.unknown.body|Mystery signal.|未知信号。|Неизвестный сигнал.|Sinal misterioso.|Mysteriöses Signal.|Gizemli sinyal.|Signal mystère.|未知信号。
hud.score|SCORE {0:000000}|分数 {0:000000}|СЧЕТ {0:000000}|SCORE {0:000000}|SCORE {0:000000}|SKOR {0:000000}|SCORE {0:000000}|スコア {0:000000}
hud.score.label|SCORE|分数|СЧЕТ|SCORE|SCORE|SKOR|SCORE|スコア
score.cache.hint|score feeds end-run dust and instant resources|分数会转化为结算星尘和即时资源|счет дает пыль в конце и ресурсы сейчас|score vira pó final e recurso imediato|Score wird zu Staub und Sofortressourcen|skor final toz ve anlık kaynak verir|le score nourrit poussière et ressources|スコアは星屑と即時資源になる
title.loop|LOOP|循环|ЦИКЛ|LOOP|SCHLEIFE|DÖNGÜ|BOUCLE|ループ
title.fighter|FIGHTER|战机|ИСТРЕБИТЕЛЬ|CAÇA|JÄGER|SAVAŞÇI|CHASSEUR|戦機
pilot.unlock.wave8|Reach wave 8|到达第 8 波|Дойти до волны 8|Alcance onda 8|Welle 8 erreichen|8. dalgaya ulaş|Atteindre vague 8|Wave 8 到達
pilot.unlock.wave16|Reach wave 16|到达第 16 波|Дойти до волны 16|Alcance onda 16|Welle 16 erreichen|16. dalgaya ulaş|Atteindre vague 16|Wave 16 到達
pilot.unlock.wave24|Reach wave 24|到达第 24 波|Дойти до волны 24|Alcance onda 24|Welle 24 erreichen|24. dalgaya ulaş|Atteindre vague 24|Wave 24 到達
capstone.chain|CAPSTONE: relays become steadier, jump farther, and finish with a shard burst.|质变：连锁更稳定，跳得更远，最终爆出碎片。|ПРЕДЕЛ: реле стабильнее, прыгают дальше и взрываются осколками.|ÁPICE: relés ficam estáveis, saltam mais e explodem em estilhaços.|KRÖNUNG: Relais sind stabiler, springen weiter und enden in Splittern.|DORUK: röleler daha sağlam, uzağa sıçrar ve parça patlatır.|APOGÉE : relais plus stables, sauts plus longs, éclats finaux.|奥義化：中継が安定し、遠くへ跳び、破片爆発で締める。
capstone.fractal|CAPSTONE: split shards can reliably create a second controlled split.|质变：分裂碎片稳定产生受控二次分裂。|ПРЕДЕЛ: осколки надежно создают второй контролируемый раскол.|ÁPICE: estilhaços criam uma segunda divisão controlada.|KRÖNUNG: Splitter erzeugen verlässlich zweite Teilungen.|DORUK: parçalar kontrollü ikinci bölünme üretir.|APOGÉE : les éclats créent une seconde fragmentation contrôlée.|奥義化：破片が安定して二段分裂を生む。
capstone.astra.refraction|CAPSTONE: prism volleys open permanent side lanes.|质变：棱镜齐射打开永久侧向弹道。|ПРЕДЕЛ: призменные залпы открывают боковые линии.|ÁPICE: salvas prisma abrem linhas laterais permanentes.|KRÖNUNG: Prismen-Salven öffnen feste Seitenbahnen.|DORUK: prizma salvoları kalıcı yan hat açar.|APOGÉE : les salves prismatiques ouvrent des lignes latérales.|奥義化：プリズム斉射が恒久側面ラインを開く。
capstone.astra.wake|CAPSTONE: Focus Prism becomes a true burst window with stronger echo fire.|质变：棱镜聚焦变成真正的爆发窗口，回声火力更强。|ПРЕДЕЛ: Фокус-призма становится окном мощного взрыва.|ÁPICE: Foco Prisma vira janela real de burst.|KRÖNUNG: Prismenfokus wird ein echtes Burst-Fenster.|DORUK: Prizma Odağı gerçek patlama penceresi olur.|APOGÉE : Focus Prisme devient une vraie fenêtre de burst.|奥義化：プリズム集中が本物のバースト窓になる。
capstone.astra.nova|CAPSTONE: Prism Focus reaches farther and emergency clear gets cheaper.|质变：棱镜聚焦范围更远，紧急清弹更便宜。|ПРЕДЕЛ: Фокус бьет дальше, очистка дешевле.|ÁPICE: Foco alcança mais e limpeza fica barata.|KRÖNUNG: Fokus reicht weiter und Noträumung wird billiger.|DORUK: Odak uzağa ulaşır, acil temizlik ucuzlar.|APOGÉE : Focus va plus loin et nettoyage coûte moins.|奥義化：プリズム集中が伸び、緊急弾消しが軽くなる。
capstone.astra.twin|CAPSTONE: Focus Prism fires a cross refraction through the arena.|质变：棱镜聚焦贯穿战场形成十字折射。|ПРЕДЕЛ: Фокус-призма режет арену крестом.|ÁPICE: Foco Prisma corta a arena em cruz.|KRÖNUNG: Prismenfokus schneidet die Arena kreuzweise.|DORUK: Prizma Odağı arenayı çapraz keser.|APOGÉE : Focus Prisme traverse l’arène en croix.|奥義化：プリズム集中が戦場を十字に貫く。
capstone.vesper.charge|CAPSTONE: the main rail leaves a delayed afterimage lance.|质变：主轨道留下延迟残影长枪。|ПРЕДЕЛ: главная рельса оставляет копье-послесвечение.|ÁPICE: o trilho principal deixa uma lança tardia.|KRÖNUNG: die Hauptschiene hinterlässt eine Nachbild-Lanze.|DORUK: ana ray gecikmeli hayalet mızrak bırakır.|APOGÉE : le rail principal laisse une lance retardée.|奥義化：主レールが遅延残像ランスを残す。
capstone.vesper.fork|CAPSTONE: rail side lanes become a stable triangular firing pattern.|质变：侧轨变成稳定三角火力结构。|ПРЕДЕЛ: боковые рельсы строят стабильный треугольник.|ÁPICE: trilhos laterais viram triângulo estável.|KRÖNUNG: Seitenbahnen formen ein stabiles Dreieck.|DORUK: yan raylar sabit üçgen ateşe döner.|APOGÉE : les rails latéraux forment un triangle stable.|奥義化：側面レールが安定した三角射撃になる。
capstone.vesper.judgment|CAPSTONE: Judgment pierces Boss guard and widens the kill lane.|质变：裁决穿透 Boss 护甲，并拓宽击杀线。|ПРЕДЕЛ: Суд пробивает защиту босса и расширяет линию.|ÁPICE: Julgamento perfura guarda de chefe e abre corredor.|KRÖNUNG: Urteil durchbohrt Bosswache und weitet die Killlinie.|DORUK: Hüküm boss korumasını deler, öldürme hattını açar.|APOGÉE : Jugement perce la garde du boss et élargit la ligne.|奥義化：審判がボス防御を貫き、撃破線を広げる。
capstone.vesper.sever|CAPSTONE: rail side lanes cross into a wider sever pattern.|质变：侧轨余震交错成更宽的裂轨结构。|ПРЕДЕЛ: боковые рельсы скрещиваются шире.|ÁPICE: trilhos laterais cruzam mais largo.|KRÖNUNG: Seitenschienen kreuzen breiter.|DORUK: yan raylar geniş kesişir.|APOGÉE : rails latéraux croisés plus larges.|奥義化：側面レールが広い断裂線に交差。
capstone.kairo.bay|CAPSTONE: drones become guardian nodes that clear bullets around themselves.|质变：无人机变成护卫节点，自动清理周围子弹。|ПРЕДЕЛ: дроны становятся стражами и чистят пули вокруг.|ÁPICE: drones viram guardiões e limpam tiros ao redor.|KRÖNUNG: Drohnen werden Wächterknoten und räumen Kugeln.|DORUK: dronlar koruma düğümü olup çevresini temizler.|APOGÉE : les drones deviennent des gardiens nettoyeurs.|奥義化：ドローンが防衛ノード化し周囲の弾を消す。
capstone.kairo.sync|CAPSTONE: swarm shots synchronize into an extra lock-on stream.|质变：蜂群射击同步为额外锁定弹流。|ПРЕДЕЛ: рой синхронизируется в дополнительный поток.|ÁPICE: tiros do enxame viram fluxo extra de mira.|KRÖNUNG: Schwarmfeuer synchronisiert zu Extra-Lock-on.|DORUK: sürü ateşi ekstra kilitli akışa dönüşür.|APOGÉE : l’essaim se synchronise en flux verrouillé.|奥義化：群体射撃が追加ロック流になる。
capstone.kairo.override|CAPSTONE: override commands refund reserve energy and sharpen swarm fire.|质变：覆写指令返还备用能量，并强化蜂群火力。|ПРЕДЕЛ: перехват возвращает энергию и усиливает рой.|ÁPICE: override devolve energia e afia o enxame.|KRÖNUNG: Override gibt Energie und schärft Schwarmfeuer.|DORUK: geçersiz kılma enerji verir, sürüyü keskinleştirir.|APOGÉE : override rend énergie et renforce l’essaim.|奥義化：上書き指令がエネルギーを返し群れを強化。
capstone.kairo.relay|CAPSTONE: relay commands refund more energy and push chain builds online.|质变：中继指令返还更多能量，让连锁流成型。|ПРЕДЕЛ: релейные приказы возвращают энергию и запускают цепи.|ÁPICE: comandos relé devolvem energia e ativam cadeias.|KRÖNUNG: Relaisbefehle geben Energie und starten Kettenbuilds.|DORUK: röle komutları enerji döndürür, zinciri açar.|APOGÉE : les ordres relais rendent énergie et lancent les chaînes.|奥義化：中継指令がエネルギーを返し連鎖を完成させる。
capstone.sol.bloom|CAPSTONE: corona scatter becomes a near-full halo.|质变：日冕散射接近完整光环。|ПРЕДЕЛ: коронный веер почти замыкает ореол.|ÁPICE: dispersão corona vira quase um halo.|KRÖNUNG: Koronastreuung wird fast zum Vollkreis.|DORUK: korona saçması neredeyse tam halka olur.|APOGÉE : la dispersion corona devient un halo quasi complet.|奥義化：コロナ散弾がほぼ全周ハローになる。
capstone.sol.forge|CAPSTONE: the forge expands the energy vessel and refills a large charge.|质变：太阳熔炉扩展能量上限，并返还大量能量。|ПРЕДЕЛ: кузня расширяет запас и возвращает заряд.|ÁPICE: forja amplia reserva e devolve carga.|KRÖNUNG: die Esse erweitert Energie und füllt viel auf.|DORUK: ocak enerji haznesini büyütür ve doldurur.|APOGÉE : la forge agrandit la réserve et rend de l’énergie.|奥義化：炉がエネルギー容量を広げ大きく回復。
capstone.sol.flare|CAPSTONE: corona skills chain flare pulses through nearby enemies.|质变：日冕技能在敌群中连锁耀斑脉冲。|ПРЕДЕЛ: коронные навыки проводят вспышки по врагам.|ÁPICE: habilidades corona encadeiam pulsos.|KRÖNUNG: Korona-Skills ketten Flare-Pulse.|DORUK: korona yetenekleri parlama darbeleri zincirler.|APOGÉE : les compétences corona chaînent des pulsations.|奥義化：コロナスキルがフレア脈動を連鎖。
capstone.sol.mantle|CAPSTONE: corona field grants a brief radiant shield.|质变：日冕场提供短暂光冕护盾。|ПРЕДЕЛ: коронное поле дает короткий сияющий щит.|ÁPICE: campo corona concede escudo radiante curto.|KRÖNUNG: Koronafeld gewährt kurzen Strahlenschild.|DORUK: korona alanı kısa ışık kalkanı verir.|APOGÉE : le champ corona donne un court bouclier radiant.|奥義化：コロナ場が短い輝光シールドを与える。
upgrade.prism.body|Adds another parallel beam. Weapon builds gain a wider base.|增加一条平行射线，让武器流派更容易铺开。|Добавляет параллельный луч и расширяет оружейную сборку.|Adiciona feixe paralelo e amplia a base da arma.|Fügt einen Parallelstrahl hinzu und verbreitert Waffenbuilds.|Paralel ışın ekler, silah yapısını genişletir.|Ajoute un rayon parallèle et élargit le build arme.|平行ビーム追加。武器ビルドの基盤が広がる。
upgrade.rail.body|Raises weapon damage and trims charge delay. Simple, brutal, beautiful.|提高武器伤害，并略微缩短蓄力间隔。|Урон оружия выше, задержка заряда ниже.|Aumenta dano e reduz atraso de carga.|Mehr Waffenschaden und kürzere Ladezeit.|Silah hasarı artar, dolum gecikmesi azalır.|Augmente les dégâts et réduit la charge.|武器ダメージ増、チャージ短縮。
upgrade.coolant.body|Faster fire cycle and a larger energy vessel for nova turns.|射击节奏更快，能量上限更高。|Огонь быстрее, запас энергии больше.|Cadência maior e reserva de energia maior.|Schnelleres Feuer und größerer Energiespeicher.|Ateş döngüsü hızlanır, enerji haznesi büyür.|Cadence plus rapide et réserve d’énergie accrue.|射撃サイクル短縮、エネルギー上限増。
upgrade.kinetic.body|Dash harder, drift faster, and carve bullets out of the air.|移动与冲刺更快，更容易切出安全通路。|Рывок и движение быстрее, путь чище.|Dash e movimento melhores para abrir caminho.|Stärkerer Sprint und bessere Fluchtwege.|Daha sert atıl, daha hızlı kaçış hattı aç.|Dash plus fort, déplacement plus rapide.|移動とダッシュ強化、安全路を切り開く。
upgrade.gravity.body|Pull pickups from farther away and thicken enemy time.|拾取范围更远，敌方节奏略微变慢。|Сбор дальше, враги немного медленнее.|Atrai drops de longe e pesa o tempo inimigo.|Sammelt weiter und verlangsamt Feindtempo.|Ganimeti uzaktan çeker, düşman zamanını ağırlaştırır.|Aimant plus large et ennemis ralentis.|遠くの拾得物を吸い、敵テンポを重くする。
upgrade.vital.body|Increases hull integrity and repairs a large chunk immediately.|提高最大生命，并立刻回复大量生命。|Больше корпуса и крупный мгновенный ремонт.|Mais casco máximo e grande reparo imediato.|Mehr Hülle und sofortige Reparatur.|Maks gövde artar, büyük onarım yapar.|Plus de coque et grosse réparation immédiate.|最大耐久増加、大きく即時回復。
upgrade.leech.body|Kills can seed repairs. Survival builds become easier to sustain.|击败敌人可能掉落修复，生存流更稳定。|Убийства могут дать ремонт, выживание стабильнее.|Abates podem gerar reparos e sustentar sobrevivência.|Kills säen Reparaturen und stabilisieren Survival.|Öldürmeler onarım doğurabilir, hayatta kalma artar.|Les tués peuvent semer des réparations.|撃破で修理が出やすく、生存が安定。
upgrade.wisp.body|Adds an orbiting shard that searches nearby targets and fires.|增加环绕碎片，自动寻找附近目标射击。|Добавляет орбитальный осколок с автопоиском цели.|Adiciona estilhaço orbital que mira sozinho.|Ein orbitierender Splitter sucht und feuert.|Dönen parça yakındaki hedefe ateş eder.|Ajoute un éclat orbital qui tire seul.|周回破片が近くの敵を自動攻撃。
upgrade.rift.body|Shots become thin piercing lances with more velocity and bite.|子弹变为更快的细长穿透弹。|Выстрелы становятся быстрыми пронзающими копьями.|Tiros viram lanças perfurantes rápidas.|Schüsse werden schnelle durchbohrende Lanzen.|Atışlar hızlı delici mızrağa döner.|Les tirs deviennent des lances perforantes.|弾が高速の細い貫通ランスになる。
upgrade.mirror.body|Reduces incoming damage and flashes absorbed force outward.|降低受到伤害，并把吸收压力向外反震。|Снижает урон и отражает накопленную силу.|Reduz dano e devolve força absorvida.|Reduziert Schaden und blitzt absorbierte Kraft zurück.|Gelen hasarı azaltır, emilen gücü dışa vurur.|Réduit les dégâts et renvoie la force absorbée.|被ダメージ軽減、吸収圧を外へ放つ。
upgrade.nova.body|Emergency clears cost less energy and your vessel holds more charge.|紧急清弹耗能更低，能量上限更高。|Очистки дешевле, запас энергии больше.|Limpezas custam menos e guardam mais carga.|Noträumung kostet weniger und speichert mehr.|Acil temizlik ucuzlar, enerji kapasitesi artar.|Nettoyage urgent moins cher, réserve plus grande.|緊急弾消しコスト減、エネルギー容量増。
upgrade.storm.body|Pilot skills release counterfire and recover faster.|角色技能释放反击弹幕，并更快冷却。|Навыки дают контрогонь и быстрее остывают.|Habilidades disparam contra-ataque e recarregam rápido.|Skills feuern zurück und laden schneller.|Yetenekler karşı ateş salar ve hızlı döner.|Les compétences ripostent et récupèrent vite.|スキルが反撃射撃し、CD短縮。
upgrade.comet.body|Dash impact damage rises and the dash clears a wider lane.|冲刺伤害更高，并清除更宽的弹幕通路。|Рывок бьет сильнее и чистит шире.|Dash causa mais dano e limpa corredor amplo.|Sprintschaden steigt und räumt breiter.|Atılma hasarı artar, daha geniş yol açar.|Dash plus violent, couloir plus large.|ダッシュ火力増、より広く弾消し。
upgrade.aegis.body|Slowly regenerates hull while you avoid damage.|一段时间不受伤会缓慢回血。|Без урона корпус медленно восстанавливается.|Sem dano por um tempo, casco regenera.|Ohne Treffer regeneriert Hülle langsam.|Hasar almazsan gövde yavaş yenilenir.|Sans dégâts, la coque se régénère.|被弾しない間、耐久が徐々に回復。
upgrade.echo.body|Shots can echo into a second ghost lance.|射击有概率追加一发幽影穿透弹。|Выстрел может породить второе призрачное копье.|Tiros podem ecoar uma lança fantasma.|Schüsse können eine Geisterlanze erzeugen.|Atışlar hayalet mızrak yankısı üretebilir.|Les tirs peuvent créer une lance fantôme.|射撃が幽霊ランスを追加することがある。
upgrade.chain.body|Weapon hits can jump to nearby enemies. Swarm and pressure waves become relay windows.|武器命中会跳向附近敌人，蜂群和压迫波收益更高。|Попадания прыгают к соседям, толпы дают окно реле.|Acertos saltam para inimigos próximos.|Treffer springen auf nahe Feinde.|İsabetler yakındaki düşmana sıçrar.|Les impacts sautent vers les ennemis proches.|命中が近くの敵へ連鎖する。
upgrade.fractal.body|Kills split into small shards. Higher ranks create controlled second splits for cleanup.|击杀会分裂出小弹，高等级可产生受控二次分裂。|Убийства создают осколки, высокие ранги дают второй раскол.|Abates geram estilhaços e segunda divisão.|Kills erzeugen Splitter und später zweite Teilung.|Öldürme parça doğurur, yüksek rütbe ikinci bölünme verir.|Les tués créent des éclats puis une seconde division.|撃破で破片弾、高ランクで二段分裂。
upgrade.solar.body|Tactical windows and overheat punishes hit harder.|战术窗口和过热破绽造成更高伤害。|Тактические окна и перегрев бьют сильнее.|Janelas táticas e superaquecimento causam mais dano.|Taktikfenster und Überhitzung treffen härter.|Taktik penceresi ve aşırı ısı daha sert vurur.|Fenêtres tactiques et surchauffe frappent plus fort.|戦術窓と過熱追撃のダメージ増。
upgrade.repair.body|Repair hull immediately and gain a little max hull.|立刻回复生命，并少量提高生命上限。|Мгновенный ремонт и немного максимума корпуса.|Repara agora e aumenta um pouco o casco.|Sofort reparieren und etwas Max-Hülle.|Anında onarır, az maks gövde verir.|Répare maintenant et augmente un peu la coque.|即時回復し、最大耐久を少し上げる。
upgrade.overdrive.body|Next wave: much higher damage. Also gain energy now.|下一波伤害大幅提高，并立刻获得能量。|Следующая волна: большой урон и энергия сейчас.|Próxima onda: muito dano e energia agora.|Nächste Welle: viel mehr Schaden und Energie.|Sonraki dalga yüksek hasar, şimdi enerji.|Prochaine vague : gros dégâts et énergie.|次Waveで大火力、今エネルギー獲得。
upgrade.glass.body|Permanent damage up, but max hull goes down.|永久提高伤害，但降低生命上限。|Постоянный урон выше, максимум корпуса ниже.|Dano permanente sobe, casco máximo desce.|Dauerhaft mehr Schaden, weniger Max-Hülle.|Kalıcı hasar artar, maks gövde düşer.|Dégâts permanents montent, coque max baisse.|恒久火力増、最大耐久低下。
upgrade.bounty.body|Next wave has more enemies, higher score-cache progress, and better drops.|下一波敌人更多，奖励推进更快，掉落更好。|Следующая волна богаче, опаснее и выгоднее.|Próxima onda tem mais inimigos e melhores drops.|Nächste Welle hat mehr Feinde und bessere Drops.|Sonraki dalga kalabalık, ödül ve ganimet iyi.|Prochaine vague plus dense, plus rentable.|次Waveは敵増加、報酬とドロップ増。
upgrade.transmute.body|Clear enemy bullets now and convert them into energy.|立即清除敌方子弹，并转化为能量。|Очищает пули и превращает их в энергию.|Limpa tiros inimigos e converte em energia.|Räumt Feindkugeln und wandelt sie in Energie.|Düşman mermisini temizler, enerjiye çevirir.|Nettoie les tirs ennemis et les convertit.|敵弾を消し、エネルギーに変換。
upgrade.map.body|Gain one extra reroll on future upgrade screens.|之后的升级界面额外获得一次重抽。|Будущие выборы получают один реролл.|Ganha um reroll extra nas próximas escolhas.|Künftige Wahlen erhalten einen Neuwurf.|Gelecek seçimlerde ekstra yenileme.|Une relance en plus aux prochains choix.|以後の強化画面で再抽選+1。
upgrade.astra.refraction.body|Astra gains extra prism lanes and a denser Focus Prism salvo.|星棱获得额外棱镜弹道，聚焦齐射更密集。|Астра получает линии призм и плотный залп.|Astra ganha linhas prisma e salva mais densa.|Astra erhält Prismenbahnen und dichtere Salven.|Astra ekstra prizma hattı ve yoğun salvo kazanır.|Astra gagne des lignes prismatiques et salves denses.|アストラのプリズムラインと集中斉射強化。
upgrade.astra.wake.body|Prism shots hit harder, cycle faster, and extend Focus Prism.|棱镜弹伤害更高、节奏更快，并延长聚焦。|Призмы бьют сильнее, чаще и дольше фокусируются.|Prismas batem mais, ciclam rápido e estendem foco.|Prismen treffen härter, schneller und länger.|Prizma atışı güçlenir, hızlanır, odağı uzatır.|Tirs prisme plus forts, plus rapides, focus prolongé.|プリズム弾が強く速くなり集中延長。
upgrade.vesper.charge.body|Rail lance damage rises. Skill lock-on cuts deeper.|轨道长枪伤害提高，技能锁定切割更深。|Урон рельсы выше, захват режет глубже.|Lança rail causa mais dano, trava corta mais.|Schienenlanze stärker, Lock schneidet tiefer.|Ray mızrağı hasarı artar, kilit derin keser.|Lance rail plus forte, verrou plus profond.|レールランス火力増、ロックが深く切る。
upgrade.vesper.fork.body|Rail shots and skill lock add narrow side lances.|轨道炮和技能锁定追加两侧细长枪线。|Рельса и захват добавляют боковые копья.|Tiros rail e trava ganham lanças laterais.|Rail und Lock erhalten Seitenlanzen.|Ray atışı ve kilit yan mızrak ekler.|Rail et verrou ajoutent des lances latérales.|レールとロックに側面ランス追加。
upgrade.kairo.bay.body|Kairo launches more orbiting drones. Skill commands a larger swarm.|环序发射更多环绕无人机，技能指挥更大蜂群。|Кайро запускает больше дронов и больший рой.|Kairo lança mais drones e comanda enxame maior.|Kairo startet mehr Drohnen und größeren Schwarm.|Kairo daha çok dron ve büyük sürü komutu alır.|Kairo lance plus de drones et un essaim plus grand.|カイロの周回ドローンと群指令拡大。
upgrade.kairo.sync.body|Drones fire faster and tactical volleys synchronize harder.|无人机射击更快，战术齐射同步更强。|Дроны стреляют быстрее, залпы синхроннее.|Drones atiram mais rápido e sincronizam salvas.|Drohnen feuern schneller und synchroner.|Dronlar hızlı ateş eder, salvolar senkron olur.|Drones plus rapides, salves mieux synchronisées.|ドローン射撃と同期斉射を強化。
upgrade.sol.bloom.body|Sol scatter fire gains more rays. Skill corona covers more space.|日冕散射弹数增加，技能覆盖范围更大。|Сол получает больше лучей и больше короны.|Sol ganha mais raios e campo corona maior.|Sol bekommt mehr Strahlen und größeres Feld.|Sol daha çok ışın ve geniş korona kazanır.|Sol gagne plus de rayons et de zone corona.|ソルの散弾数とコロナ範囲増加。
upgrade.sol.forge.body|More energy, cheaper emergency clears, and hotter corona skills.|能量更多，紧急清弹更便宜，日冕技能更强。|Больше энергии, очистка дешевле, корона горячее.|Mais energia, limpeza barata, corona mais quente.|Mehr Energie, billigere Noträumung und heißere Korona.|Daha çok enerji, ucuz temizlik, güçlü korona.|Plus d’énergie, nettoyage moins cher, corona plus vive.|エネルギー増、緊急弾消し軽量化、コロナ強化。
upgrade.astra.nova.body|Focus Prism reaches farther and emergency clear costs less.|棱镜聚焦范围更远，紧急清弹耗能更低。|Фокус бьет дальше, очистка дешевле.|Foco Prisma alcança mais e limpeza custa menos.|Prismenfokus reicht weiter und Noträumung kostet weniger.|Prizma Odağı uzağa gider, acil temizlik ucuzlar.|Focus Prisme va plus loin et nettoyage coûte moins.|プリズム集中が伸び、緊急弾消しが軽くなる。
upgrade.astra.twin.body|Focus Prism echoes with a second refraction ring and more split pressure.|棱镜聚焦追加第二圈折射，并提高分裂压制力。|Фокус получает второе кольцо и больше раскола.|Foco ganha segundo anel e mais pressão de divisão.|Fokus erhält zweiten Ring und mehr Splitterdruck.|Odak ikinci halka ve daha çok bölünme baskısı alır.|Focus gagne un second anneau et plus de fragmentation.|集中に二重屈折リングと分裂圧力を追加。
upgrade.vesper.judgment.body|Rail Judgment and skill lock become wider, cheaper, and more lethal.|轨道裁决与技能锁定更宽、更便宜、更致命。|Суд и захват шире, дешевле и смертоноснее.|Julgamento e trava ficam amplos, baratos e letais.|Urteil und Lock werden breiter, billiger, tödlicher.|Hüküm ve kilit geniş, ucuz ve ölümcül olur.|Jugement et verrou plus larges, moins chers.|審判とロックが広く軽く致命的に。
upgrade.vesper.sever.body|Skill lock and split rails create parallel aftershock beams.|技能锁定与分裂轨道生成平行余震光束。|Захват и рельсы создают параллельные лучи.|Trava e trilhos criam feixes paralelos.|Lock und Spaltschienen erzeugen parallele Nachbeben.|Kilit ve ayrık raylar paralel artçı ışın kurar.|Verrou et rails divisés créent des rayons parallèles.|ロックと分岐レールが並行余震ビームを生む。
upgrade.kairo.override.body|Override Matrix adds command bursts to the pilot skill and stronger shots.|覆写矩阵为角色技能增加指令齐射并强化弹幕。|Матрица добавляет командные залпы и урон.|Matriz adiciona rajadas e tiros mais fortes.|Matrix fügt Befehls-Salven und stärkere Schüsse hinzu.|Matris komut patlaması ve güçlü atış verir.|Matrice ajoute rafales et tirs renforcés.|上書き行列が指令斉射と弾幕強化を追加。
upgrade.kairo.relay.body|Swarm relays refund energy and make skill commands chain shots.|蜂群中继返还能量，并让技能指令连锁射击。|Рой возвращает энергию и цепляет команды.|Relés do enxame devolvem energia e encadeiam tiros.|Schwarmrelais erstatten Energie und ketten Schüsse.|Sürü rölesi enerji döndürür, komutları zincirler.|Relais d’essaim rendent énergie et chaînent les tirs.|群中継がエネルギーを返し指令射撃を連鎖。
upgrade.sol.flare.body|Corona Flare and pilot skill burn brighter and clear a wider field.|日冕耀斑和角色技能更明亮，清场范围更大。|Вспышка и навык горят ярче и шире чистят.|Flare e habilidade queimam mais e limpam maior.|Flare und Skill brennen heller und räumen breiter.|Parlama ve yetenek daha parlak, alanı geniş temizler.|Flare et compétence brûlent plus fort et nettoient large.|フレアとスキルが明るく広く掃討。
upgrade.sol.mantle.body|Radiant mantle adds hull, invulnerability, and skill recovery.|光冕护层提高生命、无敌时间，并强化技能恢复。|Мантия дает корпус, неуязвимость и откат.|Manto dá casco, invulnerabilidade e recarga.|Mantel gibt Hülle, Unverwundbarkeit und Skill-Erholung.|Örtü gövde, dokunulmazlık ve yetenek yenilenmesi verir.|Manteau donne coque, invulnérabilité et récupération.|輝光マントルで耐久、無敵、スキル回復。
pilot.nyx.name|Nyx|夜幕|Никс|Nyx|Nyx|Nyx|Nyx|ニクス
pilot.nyx.body|Gravity blades pull enemies into burst windows.|重力刃牵引敌人，制造爆发窗口。|Гравиклинки стягивают врагов к окнам урона.|Lâminas gravitam inimigos para janelas de dano.|Gravitationsklingen ziehen Feinde ins Burstfenster.|Yerçekimi bıçakları düşmanı hasar anına çeker.|Lames gravité attirent les ennemis vers le burst.|重力刃で敵を寄せ、火力窓を作る。
pilot.nyx.weapon|Gravity Blades|重力刃|Гравиклинки|Lâminas Grávitas|Gravitationsklingen|Yerçekimi Bıçakları|Lames gravité|重力刃
pilot.rook.name|Rook|壁垒|Ладья|Rook|Rook|Kale|Rook|ルーク
pilot.rook.body|Slow siege shells. Shield skill turns danger into space.|重型攻城弹。护盾技能把危险变成空间。|Медленные осадные снаряды и щит для пространства.|Projéteis de cerco lentos e escudo de espaço.|Langsame Belagerungsschüsse und Raum durch Schild.|Yavaş kuşatma mermisi, kalkanla alan açar.|Obus lents et bouclier qui ouvre l’espace.|重い攻城弾。盾で危険を空間に変える。
pilot.rook.weapon|Siege Shell|攻城重炮|Осадный снаряд|Projétil de Cerco|Belagerungsgranate|Kuşatma Mermisi|Obus de siège|攻城弾
pilot.lyra.name|Lyra|弦歌|Лира|Lyra|Lyra|Lyra|Lyra|ライラ
pilot.lyra.body|Rhythm volleys pulse in chords and reward tempo builds.|节拍齐射成和弦爆发，适合节奏流。|Ритм-залпы бьют аккордами и ценят темп.|Rajadas rítmicas em acordes para build de tempo.|Rhythmussalven in Akkorden belohnen Tempo.|Ritim salvoları akorla vurur, tempo yapısını sever.|Salves rythmiques en accords, build tempo.|リズム斉射が和音で弾け、テンポ型向き。
pilot.lyra.weapon|Pulse Chord|脉冲和弦|Пульс-аккорд|Acorde Pulsante|Impulsakkord|Darbe Akoru|Accord pulsé|パルス和音
pilot.orion.name|Orion|猎户|Орион|Orion|Orion|Orion|Orion|オリオン
pilot.orion.body|Precision spears mark priority targets and pierce deep.|精准星矛标记重点目标，深度穿透。|Точные копья метят цель и глубоко пробивают.|Lan ças precisas marcam e perfuram fundo.|Präzisionsspeere markieren und durchbohren tief.|Keskin mızraklar hedef işaretler ve deler.|Lances précises marquent et percent loin.|精密な星矛で重要目標を深く貫く。
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
tactical.nyx.tip|Pull, slow, then burst.|牵引、减速、再爆发。|Стяни, замедли, взорви.|Puxe, freie, exploda.|Ziehen, bremsen, platzen.|Çek, yavaşlat, patlat.|Attire, ralentis, explose.|引き寄せ、遅らせ、爆ぜる。
tactical.rook.name|Bulwark Drive|壁垒推进|Прорыв щитом|Avanço Baluarte|Bollwerkstoß|Siper Sürüşü|Poussée rempart|防壁突進
tactical.rook.tip|Block red fire and shove forward.|挡下红弹，向前推进。|Блокируй огонь и дави вперед.|Bloqueie tiros e avance.|Rotes Feuer blocken, vorrücken.|Kızıl ateşi tut, ileri it.|Bloque les tirs et pousse.|赤弾を止めて押し込む。
tactical.lyra.name|Beat Pulse|节拍脉冲|Ритм-пульс|Pulso de Batida|Taktpuls|Ritim Darbesi|Pulse rythmique|ビートパルス
tactical.lyra.tip|Pulse rings reward timing.|脉冲环奖励节奏。|Кольца награждают темп.|Anéis premiam ritmo.|Ringe belohnen Timing.|Halka ritmi ödüller.|Les anneaux récompensent le tempo.|輪がテンポを報いる。
tactical.orion.name|Deadeye Mark|死眼标记|Метка снайпера|Marca Certeira|Todesblick-Marke|Keskin Göz İşareti|Marque de tireur|死眼マーク
tactical.orion.tip|Mark targets, fire spears.|标记目标，发射星矛。|Меть цели, бей копьями.|Marque e lance lanças.|Ziele markieren, Speere feuern.|Hedef işaretle, mızrak at.|Marque puis lance.|標的を付け、槍を撃つ。
boss.mirror.name|Mirror Eidolon|镜像幻体|Зеркальный эйдолон|Eidolon Espelho|Spiegel-Eidolon|Ayna Eidolon|Éidolon miroir|鏡像幻体
boss.tempest.name|Tempest Wheel|风暴轮|Колесо бури|Roda Tempestade|Sturmrad|Fırtına Çarkı|Roue tempête|嵐の輪
boss.bastion.name|Bastion Gate|堡垒门|Врата бастиона|Portão Bastião|Bastiontor|Hisar Kapısı|Porte bastion|要塞門
boss.serpent.name|Coil Serpent|盘蛇|Змей-кольцо|Serpente Espiral|Schlangenspirale|Sarmal Yılan|Serpent spirale|螺旋蛇
boss.oracle.name|Oracle Needle|神谕针|Игла оракула|Agulha Oráculo|Orakelnadel|Kahin İğnesi|Aiguille oracle|神託針
upgrade.pulse.title|Pulse Magazine|脉冲弹匣|Пульс-магазин|Carregador Pulso|Impulsmagazin|Darbe Şarjörü|Chargeur pulsé|パルスマガジン
upgrade.pulse.body|Every volley adds small pulse shots. Weapon builds scale wider.|每次齐射追加小型脉冲弹，武器流更容易扩展。|Каждый залп добавляет пульс-выстрелы.|Cada salva adiciona tiros pulso.|Jede Salve fügt Impulsschüsse hinzu.|Her salvo küçük darbe atışı ekler.|Chaque salve ajoute des tirs pulsés.|斉射ごとに小型パルス弾を追加。
upgrade.execution.title|Execution Mark|处决标记|Метка казни|Marca de Execução|Exekutionsmarke|İnfaz İşareti|Marque d’exécution|処刑マーク
upgrade.execution.body|Low-hull, overheated, or guard-broken enemies take more damage.|低血、过热或破盾敌人受到更高伤害。|Слабые, перегретые и без защиты получают больше урона.|Inimigos fracos, quentes ou sem guarda sofrem mais dano.|Schwache, überhitzte oder gebrochene Feinde leiden mehr.|Düşük can, aşırı ısı veya kırık koruma daha çok hasar alır.|Ennemis faibles, surchauffés ou brisés subissent plus.|低耐久、過熱、防御破壊の敵に追加ダメージ。
upgrade.stasis.title|Stasis Field|停滞力场|Поле стазиса|Campo de Estase|Stasisfeld|Durgunluk Alanı|Champ de stase|停滞フィールド
upgrade.stasis.body|Enemy bullets and rush lines slow down. Defensive flow becomes readable.|敌方子弹与冲锋节奏变慢，防御流更清晰。|Пули и рывки врагов замедляются.|Tiros e avanços inimigos desaceleram.|Feindkugeln und Anstürme werden langsamer.|Düşman mermisi ve hücumu yavaşlar.|Tirs et charges ennemis ralentissent.|敵弾と突進が遅くなり、見切りやすい。
upgrade.magnet.title|Magnetized Core|磁化核心|Магнитное ядро|Núcleo Magnetizado|Magnetkern|Mıknatıs Çekirdek|Noyau aimanté|磁化コア
upgrade.magnet.body|Pickups pull farther and can burst into small clearing pulses.|拾取物吸得更远，并可能爆出清弹脉冲。|Сбор тянется дальше и иногда чистит пули.|Coletas atraem mais e podem pulsar limpeza.|Pickups ziehen weiter und können räumen.|Toplamalar daha uzaktan gelir, temizlik darbesi atabilir.|Collectes attirées plus loin, pulses nettoyantes.|拾得物を遠くから吸い、弾消し脈動を起こす。
upgrade.ricochet.title|Ricochet Matrix|回弹矩阵|Матрица рикошета|Matriz Ricochete|Abprallmatrix|Sekme Matrisi|Matrice ricochet|跳弾マトリクス
upgrade.ricochet.body|Hits can fire a new shard toward another target. Chains scale better.|命中可向另一个目标弹出碎片，连锁流更强。|Попадания рикошетят осколком к новой цели.|Acertos ricocheteiam estilhaço para outro alvo.|Treffer prallen als Splitter weiter.|İsabetler başka hedefe parça sektirir.|Les impacts ricochent vers une autre cible.|命中が別目標へ破片を跳弾する。
upgrade.nyx.orbit.title|Void Orbit|虚空轨道|Пустотная орбита|Órbita Vazia|Leerenorbit|Boşluk Yörüngesi|Orbite vide|虚空軌道
upgrade.nyx.orbit.body|Nyx fires more gravity blades and gains a cleaner orbit pattern.|夜幕发射更多重力刃，轨道更稳定。|Никс выпускает больше гравиклинков.|Nyx lança mais lâminas e órbita limpa.|Nyx feuert mehr Klingen und stabilere Bahnen.|Nyx daha çok bıçak ve temiz yörünge alır.|Nyx lance plus de lames et une orbite stable.|ニクスの重力刃と軌道が強化。
upgrade.nyx.singularity.title|Singularity Seed|奇点种子|Семя сингулярности|Semente Singular|Singularitätskeim|Tekillik Tohumu|Graine singulière|特異点の種
upgrade.nyx.singularity.body|Gravity Snare grows larger and hits harder around the center.|重力陷阱范围更大，中心伤害更高。|Ловушка шире и больнее в центре.|Armadilha maior e dano central maior.|Falle größer, Zentrum härter.|Tuzak büyür, merkez daha sert vurur.|Piège plus large, centre plus fort.|重力罠が広がり中心火力増。
upgrade.nyx.horizon.title|Event Horizon|事件视界|Горизонт событий|Horizonte de Evento|Ereignishorizont|Olay Ufku|Horizon d’événement|事象の地平
upgrade.nyx.horizon.body|Snared enemies slow, overheat, and feed safer energy control.|被牵引敌人减速过热，并提供更稳的能量节奏。|Пойманные враги замедляются и дают контроль энергии.|Inimigos presos desaceleram e dão controle de energia.|Gefangene verlangsamen und geben Energiekontrolle.|Yakalanan düşman yavaşlar, enerji kontrolü verir.|Ennemis piégés ralentis et meilleur contrôle d’énergie.|捕らえた敵が減速し、エネルギー管理が安定。
upgrade.nyx.cantor.title|Gravity Cantor|重力咏唱|Гравикантор|Cantor Grávito|Gravitationskantor|Yerçekimi Kantoru|Cantor gravité|重力詠唱
upgrade.nyx.cantor.body|Singularity skills spawn orbit blades and link into chain builds.|奇点技能生成轨道刃，并接入连锁流。|Навыки сингулярности дают клинки и цепи.|Singularidade cria lâminas e cadeias.|Singularität erzeugt Klingen und Ketten.|Tekillik bıçak ve zincir kurar.|Singularité crée lames et chaînes.|特異点スキルが刃と連鎖を生む。
upgrade.rook.bulwark.title|Bulwark Core|壁垒核心|Ядро бастиона|Núcleo Baluarte|Bollwerkkern|Siper Çekirdeği|Noyau rempart|防壁コア
upgrade.rook.bulwark.body|More hull, stronger shell pierce, and safer shield turns.|生命更高，重炮穿透更强，护盾更稳。|Больше корпуса, пробой и надежный щит.|Mais casco, perfuração e escudo seguro.|Mehr Hülle, Durchschlag und sicherer Schild.|Daha çok gövde, deliş ve sağlam kalkan.|Plus de coque, percée et bouclier sûr.|耐久、貫通、盾安定化。
upgrade.rook.siege.title|Siege Battery|攻城电池|Осадная батарея|Bateria de Cerco|Belagerungsbatterie|Kuşatma Bataryası|Batterie de siège|攻城バッテリー
upgrade.rook.siege.body|Siege shells hit harder and recover some fire speed.|攻城弹伤害更高，并追回部分射击速度。|Снаряды сильнее и стреляют чуть быстрее.|Obus mais fortes e cadência melhor.|Granaten stärker und etwas schneller.|Mermi daha güçlü, atış biraz hızlanır.|Obus plus forts, cadence meilleure.|攻城弾の火力と射速が改善。
upgrade.rook.aegis.title|Aegis Relay|护盾中继|Реле эгиды|Relé Égide|Aegisrelais|Kalkan Rölesi|Relais égide|イージス中継
upgrade.rook.aegis.body|Bulwark Drive restores hull and keeps energy stable.|壁垒推进回复生命，并稳定能量。|Прорыв щитом чинит корпус и держит энергию.|Avanço repara casco e mantém energia.|Bollwerkstoß heilt und stabilisiert Energie.|Siper sürüşü onarır, enerjiyi dengeler.|Poussée répare et stabilise l’énergie.|防壁突進が回復とエネルギーを支える。
upgrade.rook.citadel.title|Citadel Protocol|城垒协议|Протокол цитадели|Protocolo Cidadela|Zitadellenprotokoll|Hisar Protokolü|Protocole citadelle|城塞プロトコル
upgrade.rook.citadel.body|Shield lanes become wider and siege damage rises.|护盾通道更宽，攻城伤害更高。|Щит шире, осадный урон выше.|Escudo mais largo e dano de cerco maior.|Schildbahn breiter und Belagerungsschaden höher.|Kalkan hattı genişler, kuşatma hasarı artar.|Voie bouclier plus large et dégâts de siège accrus.|盾の道が広がり攻城火力増。
upgrade.lyra.chord.title|Resonance Chord|共鸣和弦|Резонансный аккорд|Acorde Ressonante|Resonanzakkord|Rezonans Akoru|Accord résonant|共鳴和音
upgrade.lyra.chord.body|Lyra adds chord lanes and stronger third-beat volleys.|弦歌增加和弦弹道，并强化第三拍齐射。|Лира получает аккордные линии и третий такт.|Lyra ganha linhas e terceiro tempo forte.|Lyra bekommt Akkordbahnen und dritten Takt.|Lyra akor hattı ve üçüncü vuruş alır.|Lyra gagne lignes et troisième temps fort.|ライラの和音弾道と三拍目を強化。
upgrade.lyra.tempo.title|Tempo Bloom|节拍绽放|Расцвет темпа|Florescer Tempo|Tempoblüte|Tempo Çiçeği|Floraison tempo|テンポ開花
upgrade.lyra.tempo.body|Faster rhythm, more echoes, and a backbeat safety shot.|节奏更快，回响更多，并追加反拍护身弹。|Темп быстрее, эхо больше, защитный бит.|Ritmo rápido, ecos e tiro de contratempo.|Mehr Tempo, Echos und Backbeat-Schuss.|Ritim hızlanır, yankı ve arka vuruş atar.|Tempo plus rapide, échos et tir backbeat.|テンポ、反響、裏拍弾を強化。
upgrade.lyra.cascade.title|Harmonic Cascade|谐波连瀑|Гармонический каскад|Cascata Harmônica|Harmoniekaskade|Harmonik Çağlayan|Cascade harmonique|調和連瀑
upgrade.lyra.cascade.body|Pulse rings link enemies and feed chain or split builds.|脉冲环连接敌人，强化连锁与分裂流。|Кольца связывают врагов и питают цепи.|Anéis ligam inimigos e alimentam cadeias.|Ringe verbinden Feinde und füttern Ketten.|Halkalar düşmanı bağlar, zinciri besler.|Anneaux lient les ennemis et nourrissent les chaînes.|輪が敵を結び連鎖と分裂を伸ばす。
upgrade.lyra.encore.title|Encore Field|返场力场|Поле биса|Campo Encore|Encorefeld|Encore Alanı|Champ rappel|アンコール場
upgrade.lyra.encore.body|Skills last longer, restore energy, and amplify rhythm windows.|技能持续更久，回复能量，并放大节奏窗口。|Навык дольше, дает энергию и окно темпа.|Habilidade dura mais, devolve energia e amplia ritmo.|Skills dauern länger, geben Energie und Tempo.|Yetenek uzar, enerji ve ritim penceresi verir.|Compétence plus longue, énergie et tempo.|スキル延長、エネルギー回復、テンポ窓拡大。
upgrade.orion.spear.title|Comet Spear|彗星星矛|Копье кометы|Lança Cometa|Kometenspeer|Kuyruklu Mızrak|Lance comète|彗星槍
upgrade.orion.spear.body|Main spear damage rises sharply. Boss guard breaks faster.|主星矛伤害大幅提高，更快击破 Boss 护盾。|Главное копье сильнее и ломает защиту босса.|Lança principal sobe e quebra guarda rápido.|Hauptspeer stärker, Bosswache bricht schneller.|Ana mızrak güçlenir, boss koruması hızlı kırılır.|Lance principale forte, garde boss brisée vite.|主槍火力増、Boss防御を早く割る。
upgrade.orion.deadeye.title|Deadeye Mark|死眼刻痕|Метка снайпера|Marca Certeira|Todesblick-Marke|Keskin Göz İşareti|Marque de tireur|死眼マーク
upgrade.orion.deadeye.body|Marked and wounded enemies suffer execution damage.|被标记与低血敌人承受处决伤害。|Меченые и слабые враги получают казнь.|Marcados e feridos sofrem execução.|Markierte und verwundete Feinde erleiden Exekution.|İşaretli ve yaralı düşman infaz hasarı alır.|Marqués et blessés subissent exécution.|標的と瀕死敵に処刑ダメージ。
upgrade.orion.quiver.title|Starfall Quiver|星陨箭匣|Колчан звездопада|Aljava Estelar|Sternfallköcher|Yıldız Okluğu|Carquois stellaire|星落の矢筒
upgrade.orion.quiver.body|Side spears create a sniper spread without losing precision.|侧向星矛形成狙击散射，同时保持精准。|Боковые копья дают точный веер.|Lan ças laterais espalham sem perder precisão.|Seitenspeere streuen ohne Präzisionsverlust.|Yan mızraklar isabetli saçılır.|Lances latérales, dispersion précise.|側面槍で精密な散射を作る。
upgrade.orion.perihelion.title|Perihelion Vector|近日点矢量|Вектор перигелия|Vetor Periélio|Perihelvektor|Günberi Vektörü|Vecteur périhélie|近日点ベクトル
upgrade.orion.perihelion.body|Skill resets movement tempo and lets Orion reposition after marks.|技能重置移动节奏，让猎户标记后换位。|Навык сбрасывает темп и дает смену позиции.|Habilidade reseta movimento para reposicionar.|Skill setzt Tempo zurück und repositioniert.|Yetenek hareket temposunu yeniler, yer değiştirir.|Compétence relance le tempo et replace Orion.|スキルで移動テンポを戻し位置替え。
capstone.pulse|CAPSTONE: pulse shots gain an extra lane and tighten the weapon flow.|质变：脉冲弹追加一条弹道，武器流更紧凑。|ПРЕДЕЛ: пульс-выстрелы получают линию.|ÁPICE: tiros pulso ganham faixa extra.|KRÖNUNG: Impulsschüsse erhalten Zusatzbahn.|DORUK: darbe atışı ekstra hat kazanır.|APOGÉE : tirs pulsés gagnent une voie.|奥義化：パルス弾に追加レーン。
capstone.execution|CAPSTONE: execution damage spikes when Boss guard is broken.|质变：Boss 破盾时处决伤害爆发。|ПРЕДЕЛ: казнь резко сильнее по сломанной защите.|ÁPICE: execução explode em guarda quebrada.|KRÖNUNG: Exekution platzt bei Bossbruch.|DORUK: koruma kırılınca infaz patlar.|APOGÉE : exécution explose sur garde brisée.|奥義化：Boss防御破壊時に処刑火力増。
capstone.stasis|CAPSTONE: a full-screen stasis purge clears red fire once.|质变：停滞净化会全屏清除一次红弹。|ПРЕДЕЛ: стазис раз очищает красный огонь.|ÁPICE: estase limpa fogo vermelho uma vez.|KRÖNUNG: Stasis räumt rotes Feuer einmal.|DORUK: durgunluk kızıl ateşi bir kez temizler.|APOGÉE : stase nettoie le feu rouge une fois.|奥義化：停滞で赤弾を一度全消去。
capstone.magnet|CAPSTONE: pickups pull from far away and trigger more pulses.|质变：拾取物吸附更远，触发更多清场脉冲。|ПРЕДЕЛ: сбор тянется дальше и чаще пульсирует.|ÁPICE: coletas puxam longe e pulsam mais.|KRÖNUNG: Pickups ziehen weiter und pulsen öfter.|DORUK: toplama uzaktan gelir, daha çok darbe atar.|APOGÉE : collectes attirées loin, pulses fréquents.|奥義化：拾得物吸引と脈動が強化。
capstone.ricochet|CAPSTONE: ricochets can bounce twice and start relay chains.|质变：回弹可二次跳跃，并启动中继连锁。|ПРЕДЕЛ: рикошет дважды прыгает и запускает цепи.|ÁPICE: ricochetes saltam duas vezes e ligam cadeias.|KRÖNUNG: Abpraller springen doppelt und starten Ketten.|DORUK: sekme iki kez sıçrar ve zincir başlatır.|APOGÉE : ricochets doubles et chaînes relais.|奥義化：跳弾が二段化し連鎖を起動。
capstone.nyx.orbit|CAPSTONE: gravity blades form a stable twin orbit.|质变：重力刃形成稳定双轨道。|ПРЕДЕЛ: клинки образуют двойную орбиту.|ÁPICE: lâminas formam órbita dupla.|KRÖNUNG: Klingen bilden Doppelorbit.|DORUK: bıçaklar çift yörünge kurar.|APOGÉE : lames en double orbite.|奥義化：重力刃が二重軌道化。
capstone.nyx.singularity|CAPSTONE: singularities pull harder and refund weapon tempo.|质变：奇点牵引更强，并返还武器节奏。|ПРЕДЕЛ: сингулярность тянет сильнее.|ÁPICE: singularidades puxam mais e devolvem ritmo.|KRÖNUNG: Singularitäten ziehen stärker.|DORUK: tekillik daha sert çeker.|APOGÉE : singularités attirent plus fort.|奥義化：特異点の牽引強化。
capstone.nyx.horizon|CAPSTONE: Event Horizon turns trapped enemies into overheat windows.|质变：事件视界把被困敌人变成过热窗口。|ПРЕДЕЛ: горизонт делает врагов перегретыми.|ÁPICE: horizonte cria janelas de superaquecimento.|KRÖNUNG: Horizont macht Überhitzungsfenster.|DORUK: ufuk aşırı ısı penceresi açar.|APOGÉE : horizon crée fenêtres de surchauffe.|奥義化：捕縛敵が過熱窓になる。
capstone.nyx.cantor|CAPSTONE: singularity skills throw orbit blades into chain arcs.|质变：奇点技能把轨道刃接入连锁电弧。|ПРЕДЕЛ: клинки входят в цепные дуги.|ÁPICE: lâminas entram em arcos de cadeia.|KRÖNUNG: Orbitklingen starten Kettenbögen.|DORUK: yörünge bıçağı zincir arkı kurar.|APOGÉE : lames alimentent les arcs de chaîne.|奥義化：軌道刃が連鎖アークへ。
capstone.rook.bulwark|CAPSTONE: Rook gains a larger hull buffer and safer shield casts.|质变：壁垒获得更大生命缓冲，护盾更安全。|ПРЕДЕЛ: больше корпуса и безопасный щит.|ÁPICE: mais casco e escudo seguro.|KRÖNUNG: mehr Hülle und sicherer Schild.|DORUK: daha çok gövde, güvenli kalkan.|APOGÉE : plus de coque, bouclier sûr.|奥義化：耐久と盾の安全性強化。
capstone.rook.siege|CAPSTONE: siege shells hit like artillery and carry the build.|质变：攻城弹变成主力炮击核心。|ПРЕДЕЛ: осадные снаряды становятся артиллерией.|ÁPICE: obus viram artilharia central.|KRÖNUNG: Granaten werden Artilleriekern.|DORUK: kuşatma mermisi topçu çekirdeği olur.|APOGÉE : obus deviennent artillerie centrale.|奥義化：攻城弾が砲撃核心に。
capstone.rook.aegis|CAPSTONE: Aegis Relay restores more hull during shield turns.|质变：护盾中继在防守回合回复更多生命。|ПРЕДЕЛ: эгида сильнее чинит щитовые ходы.|ÁPICE: égide repara mais no escudo.|KRÖNUNG: Aegis heilt Schildrunden stärker.|DORUK: kalkan turunda daha çok onarır.|APOGÉE : égide répare plus sous bouclier.|奥義化：盾中の回復強化。
capstone.rook.citadel|CAPSTONE: citadel protocol turns siege shells into the main damage plan.|质变：城垒协议让攻城弹成为主力输出核心。|ПРЕДЕЛ: протокол делает осадные снаряды ядром урона.|ÁPICE: protocolo faz obus virarem dano central.|KRÖNUNG: Protokoll macht Granaten zum Schadenskern.|DORUK: protokol kuşatma mermisini ana hasar yapar.|APOGÉE : protocole fait des obus le cœur des dégâts.|奥義化：城塞プロトコルで攻城弾が主火力に。
capstone.lyra.chord|CAPSTONE: every third beat becomes a wide chord burst.|质变：每第三拍变成宽幅和弦爆发。|ПРЕДЕЛ: каждый третий такт широкий аккорд.|ÁPICE: terceiro tempo vira acorde amplo.|KRÖNUNG: jeder dritte Takt wird Breitakkord.|DORUK: her üçüncü vuruş geniş akor olur.|APOGÉE : chaque troisième temps explose large.|奥義化：三拍目が広い和音爆発に。
capstone.lyra.tempo|CAPSTONE: tempo blooms into a backbeat safety shot.|质变：节拍绽放为反拍护身弹。|ПРЕДЕЛ: темп дает защитный бэкбит.|ÁPICE: tempo cria tiro backbeat.|KRÖNUNG: Tempo erzeugt Backbeat-Schutz.|DORUK: tempo arka vuruş savunması verir.|APOGÉE : tempo crée un tir backbeat.|奥義化：裏拍護身弾を得る。
capstone.lyra.cascade|CAPSTONE: cascade pulses feed chain and split builds at once.|质变：谐波脉冲同时喂养连锁与分裂流。|ПРЕДЕЛ: каскад питает цепь и раскол.|ÁPICE: cascata alimenta cadeia e divisão.|KRÖNUNG: Kaskade speist Kette und Splitter.|DORUK: çağlayan zincir ve bölünme besler.|APOGÉE : cascade nourrit chaînes et éclats.|奥義化：連鎖と分裂を同時強化。
capstone.lyra.encore|CAPSTONE: Encore grants longer rhythm windows and more echoes.|质变：返场提供更久节奏窗口与更多回响。|ПРЕДЕЛ: бис дает окна и эхо дольше.|ÁPICE: encore amplia janela e ecos.|KRÖNUNG: Encore verlängert Fenster und Echos.|DORUK: encore ritim ve yankıyı uzatır.|APOGÉE : rappel prolonge fenêtre et échos.|奥義化：テンポ窓と反響を延長。
capstone.orion.spear|CAPSTONE: comet spears carve through Boss guard faster.|质变：彗星矛更快撕开 Boss 护盾。|ПРЕДЕЛ: копья быстрее режут защиту босса.|ÁPICE: lanças quebram guarda de chefe rápido.|KRÖNUNG: Speere brechen Bosswache schneller.|DORUK: mızrak boss korumasını hızlı yıkar.|APOGÉE : lances percent la garde boss vite.|奥義化：Boss防御をより速く割る。
capstone.orion.deadeye|CAPSTONE: marked targets ignite a precision chain arc.|质变：被标记目标引发精准连锁电弧。|ПРЕДЕЛ: метки запускают точную дугу.|ÁPICE: marcas geram arco preciso.|KRÖNUNG: Marken starten Präzisionsbogen.|DORUK: işaret hassas zincir arkı başlatır.|APOGÉE : marques lancent un arc précis.|奥義化：標的から精密連鎖アーク。
capstone.orion.quiver|CAPSTONE: Starfall shots reset dash tempo and return reserve energy.|质变：星陨弹重置冲刺节奏，并返还备用能量。|ПРЕДЕЛ: звездные выстрелы сбрасывают рывок и дают энергию.|ÁPICE: tiros estelares resetam dash e devolvem energia.|KRÖNUNG: Sternschüsse setzen Sprint zurück und geben Energie.|DORUK: yıldız atışı atılmayı yeniler ve enerji verir.|APOGÉE : tirs stellaires relancent le dash et rendent énergie.|奥義化：星落弾でダッシュテンポ回復とエネルギー還元。
capstone.orion.perihelion|CAPSTONE: Perihelion resets dash tempo after mark skills.|质变：近日点在标记技能后重置冲刺节奏。|ПРЕДЕЛ: перигелий сбрасывает рывок после метки.|ÁPICE: periélio reseta dash após marca.|KRÖNUNG: Perihel setzt Sprint nach Marke zurück.|DORUK: günberi işaret sonrası atılmayı yeniler.|APOGÉE : périhélie relance le dash après marque.|奥義化：マーク後にダッシュテンポ回復。
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
                GameLanguage.Russian => "ПРЕДЕЛ: цепи прыгают дальше и запускают финальный осколочный взрыв.",
                GameLanguage.PortugueseBrazil => "ÁPICE: cadeias saltam mais longe e terminam em estilhaços.",
                GameLanguage.German => "KRÖNUNG: Ketten springen weiter und enden in Splitterexplosionen.",
                GameLanguage.Turkish => "DORUK: zincirler daha uzağa sıçrar ve parça patlaması yapar.",
                GameLanguage.French => "APOGÉE : les chaînes vont plus loin et finissent en éclats.",
                GameLanguage.Japanese => "奥義化：連鎖が遠くへ跳び、最後に破片爆発。",
                _ => "CAPSTONE: chains jump farther and end in a shard burst.",
            };
        }

        if (key.Contains("split", StringComparison.Ordinal) || key.Contains("fractal", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "ПРЕДЕЛ: расколы надежно создают второй контролируемый взрыв.",
                GameLanguage.PortugueseBrazil => "ÁPICE: divisões criam uma segunda ruptura controlada.",
                GameLanguage.German => "KRÖNUNG: Splitter erzeugen verlässlich eine zweite Teilung.",
                GameLanguage.Turkish => "DORUK: bölünmeler güvenilir ikinci parçalanma üretir.",
                GameLanguage.French => "APOGÉE : les fragments créent une seconde division contrôlée.",
                GameLanguage.Japanese => "奥義化：分裂が安定して二段分裂を生む。",
                _ => "CAPSTONE: split shards reliably create a second controlled split.",
            };
        }

        if (key.Contains("astra", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "ПРЕДЕЛ: призмы открывают настоящие окна взрывного огня.",
                GameLanguage.PortugueseBrazil => "ÁPICE: prismas abrem janelas reais de explosão.",
                GameLanguage.German => "KRÖNUNG: Prismen öffnen echte Burst-Fenster.",
                GameLanguage.Turkish => "DORUK: prizmalar gerçek patlama penceresi açar.",
                GameLanguage.French => "APOGÉE : les prismes ouvrent une vraie fenêtre de burst.",
                GameLanguage.Japanese => "奥義化：プリズムが本物のバースト窓になる。",
                _ => "CAPSTONE: prism fire becomes a true burst window.",
            };
        }

        if (key.Contains("vesper", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "ПРЕДЕЛ: рельсы оставляют смертоносные линии послесвечения.",
                GameLanguage.PortugueseBrazil => "ÁPICE: trilhos deixam linhas tardias letais.",
                GameLanguage.German => "KRÖNUNG: Schienen hinterlassen tödliche Nachbild-Lanzen.",
                GameLanguage.Turkish => "DORUK: raylar ölümcül ardıl çizgiler bırakır.",
                GameLanguage.French => "APOGÉE : les rails laissent des lignes retardées mortelles.",
                GameLanguage.Japanese => "奥義化：レールが遅延する殺傷線を残す。",
                _ => "CAPSTONE: rail fire leaves lethal afterimage lines.",
            };
        }

        if (key.Contains("kairo", StringComparison.Ordinal))
        {
            return _language switch
            {
                GameLanguage.Russian => "ПРЕДЕЛ: рой помечает врагов и возвращает больше энергии.",
                GameLanguage.PortugueseBrazil => "ÁPICE: o enxame marca inimigos e devolve energia.",
                GameLanguage.German => "KRÖNUNG: der Schwarm markiert Feinde und erstattet Energie.",
                GameLanguage.Turkish => "DORUK: sürü düşmanları işaretler ve enerji döndürür.",
                GameLanguage.French => "APOGÉE : l’essaim marque les ennemis et rend de l’énergie.",
                GameLanguage.Japanese => "奥義化：群れが敵を標識し、エネルギーを返す。",
                _ => "CAPSTONE: swarm commands mark enemies and refund energy.",
            };
        }

        return _language switch
        {
            GameLanguage.Russian => "ПРЕДЕЛ: корона расширяет поле, щит и очистку.",
            GameLanguage.PortugueseBrazil => "ÁPICE: a corona amplia campo, escudo e limpeza.",
            GameLanguage.German => "KRÖNUNG: Korona verstärkt Feld, Schild und Räumen.",
            GameLanguage.Turkish => "DORUK: korona alanı, kalkanı ve temizliği büyütür.",
            GameLanguage.French => "APOGÉE : la corona amplifie champ, bouclier et nettoyage.",
            GameLanguage.Japanese => "奥義化：コロナが場、防護、弾消しを広げる。",
            _ => "CAPSTONE: corona power expands the field, shield, and clear.",
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
