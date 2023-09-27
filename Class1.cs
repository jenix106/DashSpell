using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace DashSpell
{
    public class DashSettings : ThunderScript
    {
        [ModOption(name: "Dash Speed", tooltip: "The speed of the dash", valueSourceName: nameof(largeValues), defaultValueIndex = 40, order = 1)]
        public static float DashSpeed = 2000;
        [ModOption(name: "Disable Gravity", tooltip: "Disables gravity while dashing", valueSourceName: nameof(booleanOption), defaultValueIndex = 0, order = 2)]
        public static bool DisableGravity = true;
        [ModOption(name: "Disable Body Collision", tooltip: "Disables collisions on your body while dashing", valueSourceName: nameof(booleanOption), defaultValueIndex = 0, order = 3)]
        public static bool DisableBodyCollision = true;
        [ModOption(name: "Disable Weapon/Hand Collision", tooltip: "Disables collisions on your weapons & hands while dashing", valueSourceName: nameof(booleanOption), defaultValueIndex = 0, order = 4)]
        public static bool DisableWeaponCollision = true;
        [ModOption(name: "Dash Time", tooltip: "How long the dash lasts, in seconds", valueSourceName: nameof(hundrethsValues), defaultValueIndex = 25, order = 5)]
        public static float DashTime = 0.25f;
        [ModOption(name: "Stop On End", tooltip: "Stops momentum at the end of a dash", valueSourceName: nameof(booleanOption), defaultValueIndex = 0, order = 6)]
        public static bool StopOnEnd = true;
        [ModOption(name: "Stop On Start", tooltip: "Stops momentum at the start of a dash", valueSourceName: nameof(booleanOption), defaultValueIndex = 0, order = 7)]
        public static bool StopOnStart = true;
        [ModOption(name: "Thumbstick Dash", tooltip: "Dashes in the same direction you're moving", valueSourceName: nameof(booleanOption), defaultValueIndex = 0, order = 8)]
        public static bool ThumbstickDash = true;
        [ModOption(name: "Dash Real Time", tooltip: "Disregards slow motion when dashing", valueSourceName: nameof(booleanOption), defaultValueIndex = 1, order = 9)]
        public static bool DashRealTime = false;
        public static ForceMode DashForceMode = ForceMode.Impulse; 
        public static ModOptionBool[] booleanOption =
         {
            new ModOptionBool("Enabled", true),
            new ModOptionBool("Disabled", false)
        };
        public static ModOptionString[] forceOption =
        {
            new ModOptionString("Acceleration", "Acceleration"),
            new ModOptionString("Force", "Force"),
            new ModOptionString("Impulse", "Impulse"),
            new ModOptionString("Velocity Change", "VelocityChange")
        };
        [ModOption(name: "Dash Force Mode", tooltip: "The force mode of the dash", valueSourceName: nameof(forceOption), defaultValueIndex = 2, order = 10)]
        public static void DashForceOptions(string option)
        {
            switch (option)
            {
                case "Force":
                    DashForceMode = ForceMode.Force;
                    break;
                case "Acceleration":
                    DashForceMode = ForceMode.Acceleration;
                    break;
                case "Impulse":
                    DashForceMode = ForceMode.Impulse;
                    break;
                case "VelocityChange":
                    DashForceMode = ForceMode.VelocityChange;
                    break;
            }
        }
        public static ModOptionFloat[] hundrethsValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0.00"), num);
                num += 0.01f;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] largeValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0"), num);
                num += 50f;
            }
            return modOptionFloats;
        }
    }
    public class DashSpell : SpellCastCharge
    {
        public static bool fallDamage;
        public static Coroutine dash = null;
        public static bool dashing = false;
        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active)
            {
                if (dash != null)
                    spellCaster.StopCoroutine(dash);
                dash = spellCaster.StartCoroutine(Dash());
            }
        }
        public override void UpdateCaster()
        {
            base.UpdateCaster();
            if (!dashing) fallDamage = Player.fallDamage;
        }
        public IEnumerator Dash()
        {
            dashing = true;
            Player.fallDamage = false;
            if (DashSettings.StopOnStart) Player.local.locomotion.rb.velocity = Vector3.zero;
            if (Player.local.locomotion.moveDirection.magnitude <= 0 || !DashSettings.ThumbstickDash)
                Player.local.locomotion.rb.AddForce(Player.local.head.transform.forward * (!DashSettings.DashRealTime ? DashSettings.DashSpeed : DashSettings.DashSpeed / Time.timeScale), DashSettings.DashForceMode);
            else
            {
                Player.local.locomotion.rb.AddForce(Player.local.locomotion.moveDirection.normalized * (!DashSettings.DashRealTime ? DashSettings.DashSpeed : DashSettings.DashSpeed / Time.timeScale), DashSettings.DashForceMode);
            }
            if (DashSettings.DisableGravity)
                Player.local.locomotion.rb.useGravity = false;
            if (DashSettings.DisableBodyCollision)
            {
                Player.local.locomotion.rb.detectCollisions = false;
            }
            if (DashSettings.DisableWeaponCollision)
            {
                if (spellCaster.ragdollHand?.grabbedHandle?.item is Item item)
                    item.physicBody.rigidBody.detectCollisions = false;
                spellCaster.ragdollHand.physicBody.rigidBody.detectCollisions = false;
                spellCaster.ragdollHand.otherHand.physicBody.rigidBody.detectCollisions = false;
            }
            if (DashSettings.DashRealTime) yield return new WaitForSecondsRealtime(DashSettings.DashTime);
            else yield return new WaitForSeconds(DashSettings.DashTime);
            if (DashSettings.DisableGravity)
                Player.local.locomotion.rb.useGravity = true;
            if (DashSettings.DisableBodyCollision)
            {
                Player.local.locomotion.rb.detectCollisions = true;
            }
            if (DashSettings.DisableWeaponCollision)
            {
                if (spellCaster.ragdollHand?.grabbedHandle?.item is Item item)
                    item.physicBody.rigidBody.detectCollisions = true;
                spellCaster.ragdollHand.physicBody.rigidBody.detectCollisions = true;
                spellCaster.ragdollHand.otherHand.physicBody.rigidBody.detectCollisions = true;
            }
            if (DashSettings.StopOnEnd) Player.local.locomotion.rb.velocity = Vector3.zero;
            Player.fallDamage = fallDamage;
            dashing = false;
            yield break;
        }
    }
}
