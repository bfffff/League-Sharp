using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
 
namespace Fiora
{
    class Program
    {
        private const string ChampName = "Fiora";
 
        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
 
        private static Spell Q, W, E, R;
 
        // Irelia Ultimate stuff.
        private static bool _hasToFire;
        private static int _charges = 0;
 
        private static bool _packetCast;
 
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
 
        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != ChampName)
                return;
 
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, ObjectManager.Player.AttackRange); // So confused.
            E = new Spell(SpellSlot.E, ObjectMenager.Player.AttackRange);
            R = new Spell(SpellSlot.R, 400);
 
            Q.SetSkillshot(0.25f, 75f, 1500f, false, Prediction.SkillshotType.SkillshotLine);
            R.SetSkillshot(0.15f, 80f, 1500f, false, Prediction.SkillshotType.SkillshotLine);
 
 
 
        static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = _menu.Item("qDraw").GetValue<bool>();
            var drawE = _menu.Item("eDraw").GetValue<bool>();
            var drawR = _menu.Item("rDraw").GetValue<bool>();
 
            var position = ObjectManager.Player.Position;
 
            if(drawQ)
                Utility.DrawCircle(position, Q.Range, Color.Gray);
 
            if(drawE)
                Utility.DrawCircle(position, E.Range, Color.Gray);
 
            if(drawR)
                Utility.DrawCircle(position, R.Range, Color.Gray);
        }
 
        private static void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!_menu.Item("interruptUlts").GetValue<bool>()) return;
 
            String[] spellsToInterrupt =
            {
                "AbsoluteZero",
                "AlZaharNetherGrasp",
                "CaitlynAceintheHole",
                "Crowstorm",
                "DrainChannel",
                "FallenOne",
                "GalioIdolOfDurand",
                "InfiniteDuress",
                "KatarinaR",
                "MissFortuneBulletTime",
                "Teleport",
                "Pantheon_GrandSkyfall_Jump",
                "ShenStandUnited",
                "UrgotSwap2"
            };
 
            var spellName = args.SData.Name;
            var target = sender;
 
            foreach (var spell in spellsToInterrupt.Where(spell => spell == spellName))
            {
                if (_menu.Item("interruptQE").GetValue<bool>())
                {
                    if (!CanStunTarget(target)) continue;
                    if (Q.IsReady()) Q.Cast(target, _packetCast);
                    if (E.IsReady()) E.Cast(target, _packetCast);
                }
                else
                {
                    if (!CanStunTarget(target)) continue;
                    if (E.IsReady()) E.Cast(target, _packetCast);
                }
            }
        }
 
        static void Game_OnGameUpdate(EventArgs args)
        {
            _packetCast = _menu.Item("packetCast").GetValue<bool>();
 
            FireCharges();
 
            if (!Orbwalking.CanMove(100)) return;
 
            if (_menu.Item("waveClear").GetValue<KeyBind>().Active && !ObjectManager.Player.IsDead)
            {
                WaveClear();
            }
 
            if (_menu.Item("comboActive").GetValue<KeyBind>().Active && !ObjectManager.Player.IsDead)
            {
                Combo();
            }
 
        }
 
        private static void WaveClear()
        {
            var useE = _menu.Item("useEEC").GetValue<bool>();
            
        }
 
 
 
        private static void FireCharges()
        {
            if (!_hasToFire) return;
 
            R.Cast(SimpleTs.GetTarget(400, SimpleTs.DamageType.Physical), _packetCast); //Dunnno
            _charges -= 1;
            _hasToFire = _charges != 0;
        }
 
        private static void Combo()
        {
            // Simple combo q -> w -> e -> r
            var useQ = _menu.Item("useQ").GetValue<bool>();
            var useQ = _menu.Item("useQ").GetValue<bool>();
            var useW = _menu.Item("useW").GetValue<bool>();
            var useE = _menu.Item("useE").GetValue<bool>();
            var useR = _menu.Item("useR").GetValue<bool>();
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            
            if (target == null || !target.IsValid) return;
 
            var isUnderTower = Utility.UnderTurret(target);
            var diveTower = _menu.Item("diveTower").GetValue<bool>();
            var doNotCombo = false;
 
            // if target is under tower, and we do not want to dive
            if (isUnderTower && !diveTower)
            {
                // Calculate percent hp
                var percent = (int) target.Health/target.MaxHealth*100;
                var overridePercent = _menu.Item("diveTowerPercent").GetValue<Slider>().Value;
 
                if (percent > overridePercent) doNotCombo = true;
 
            }
 
            if (doNotCombo) return;
 
            if (useW && W.IsReady())
            {
                W.Cast();
            }
 
            // follow up with q
            if (useQ && Q.IsReady())
            {
                if (_menu.Item("dontQ").GetValue<bool>())
                {
                    var distance = ObjectManager.Player.Distance(target);
 
                    if (distance > _menu.Item("dontQRange").GetValue<Slider>().Value)
                    {
                        Q.Cast(target, _packetCast);
                    }
                }
                else
                {
                    Q.Cast(target, _packetCast);
                }
            }
 
 
 

 
        private static void SetupMenu()
        {
            _menu = new Menu("--[Fiora]--", "cmIrelia", true);
 
            // Target Selector
            var targetSelectorMenu = new Menu("[Fiora] - TS", "cmIreliaTS");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);
 
            // Orbwalker
            var orbwalkerMenu = new Menu("[Fiora] - Orbwalker", "cmIreliaOW");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);
 
            // Combo
            var comboMenu = new Menu("[Fiora] - Combo", "cmIreliaCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use e in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R in combo").SetValue(true));
            _menu.AddSubMenu(comboMenu);
            
            // Lasthiting
            var farmingMenu = new Menu("[fiora] - Farming", "cmIreliaFarming");
            // Wave clear submenu
            var waveClearMenu = new Menu("Wave Clear", "cmIreliaFarmingWaveClear");
            waveClearMenu.AddItem(new MenuItem("useEWC", "Use E").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveClear", "Wave Clear!").SetValue(new KeyBind(86, KeyBindType.Press)));
            farmingMenu.AddSubMenu(waveClearMenu);
            _menu.AddSubMenu(farmingMenu);
 
            //Drawing menu
            var drawingMenu = new Menu("[fiora] - Drawing", "cmIreliaDraw");
            drawingMenu.AddItem(new MenuItem("qDraw", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("eDraw", "Draw E").SetValue(false));
            drawingMenu.AddItem(new MenuItem("rDraw", "Draw R").SetValue(true));
            _menu.AddSubMenu(drawingMenu);
 
            //Misc
            var miscMenu = new Menu("[fiora - Misc", "cmIreliaMisc");
            miscMenu.AddItem(new MenuItem("interruptUlts", "Interrupt ults with E").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptQE", "Q + E to interrupt if not in range").SetValue(true));
            miscMenu.AddItem(new MenuItem("packetCast", "Use packets to cast spells").SetValue(false));
            miscMenu.AddItem(new MenuItem("diveTower", "Dive tower when combo'ing").SetValue(false));
            miscMenu.AddItem(new MenuItem("diveTowerPercent", "Override dive tower").SetValue(new Slider(10)));
            miscMenu.AddItem(new MenuItem("dontQ", "Dont Q if range is small").SetValue(false));
            miscMenu.AddItem(new MenuItem("dontQRange", "Q Range").SetValue(new Slider(200, 0, 600)));
            _menu.AddSubMenu(miscMenu);
 
            // Use combo, last hit, c
            _menu.AddItem(new MenuItem("comboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
 
            // Finalize
            _menu.AddToMainMenu();
        }
    }
