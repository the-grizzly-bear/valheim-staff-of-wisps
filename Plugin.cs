using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace StaffOfWisps
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.jotunn.jotunn")]
    public class StaffOfWispsPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "mishka.valheim.staffofwisps";
        public const string PluginName = "StaffOfWisps";
        public const string PluginVersion = "1.3.0";

        private static readonly Color WispColor = new Color(0.55f, 0.85f, 1f);
        private static readonly float[] StayTtlByQuality = { 25f, 35f, 45f, 60f };
        private static readonly float[] MistRadiusMultiplierByQuality = { 1f, 1.25f, 1.5f, 2f };

        private void Awake()
        {
            new Harmony(PluginGUID).PatchAll();
            PrefabManager.OnVanillaPrefabsAvailable += AddStaffOfWisps;
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.Setup))]
        private static class ScaleStayTtlWithQuality
        {
            private static void Postfix(Projectile __instance, ItemDrop.ItemData item)
            {
                if (item?.m_dropPrefab == null || item.m_dropPrefab.name != "StaffWisp")
                {
                    return;
                }

                int index = Mathf.Clamp(item.m_quality - 1, 0, StayTtlByQuality.Length - 1);
                __instance.m_stayTTL = StayTtlByQuality[index];

                ParticleSystemForceField forceField = __instance.GetComponentInChildren<ParticleSystemForceField>();
                if (forceField != null)
                {
                    forceField.endRange *= MistRadiusMultiplierByQuality[index];
                }
            }
        }

        private void AddStaffOfWisps()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddStaffOfWisps;

            GameObject projectile = BuildWispProjectile();
            GameObject item = BuildWispStaff(projectile);

            ItemManager.Instance.AddItem(new CustomItem(item, fixReference: true));

            ItemManager.Instance.AddRecipe(new CustomRecipe(new RecipeConfig
            {
                Name = "Recipe_StaffWisp",
                Item = "StaffWisp",
                Amount = 1,
                CraftingStation = "piece_magetable",
                Requirements = new[]
                {
                    new RequirementConfig { Item = "YggdrasilWood", Amount = 20, AmountPerLevel = 10 },
                    new RequirementConfig { Item = "Wisp", Amount = 4, AmountPerLevel = 2 },
                    new RequirementConfig { Item = "Eitr", Amount = 16, AmountPerLevel = 8 },
                }
            }));
        }

        private GameObject BuildWispProjectile()
        {
            GameObject projectile = PrefabManager.Instance.CreateClonedPrefab(
                "staff_wisp_projectile", "staff_fireball_projectile");

            ReplaceFireLookWithWisp(projectile.transform);
            AttachDemisterOrb(projectile.transform, Vector3.zero, 0.5f, keepMistClearing: true);

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.m_gravity = 3.5f;
                proj.m_stayAfterHitStatic = true;
                proj.m_stayAfterHitDynamic = true;
                proj.m_stayTTL = 25f;
                proj.m_ttl = 30f;

                if (proj.m_hitEffects.m_effectPrefabs != null)
                {
                    for (int i = 0; i < proj.m_hitEffects.m_effectPrefabs.Length; i++)
                    {
                        proj.m_hitEffects.m_effectPrefabs[i].m_enabled = false;
                    }
                }
            }

            return projectile;
        }

        private GameObject BuildWispStaff(GameObject projectile)
        {
            GameObject item = PrefabManager.Instance.CreateClonedPrefab("StaffWisp", "StaffFireball");

            ItemDrop itemDrop = item.GetComponent<ItemDrop>();
            itemDrop.m_itemData.m_shared.m_name = "Staff of Wisps";
            itemDrop.m_itemData.m_shared.m_description = "Throw a bound wisp ahead of you to light the way through the mist.";
            itemDrop.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;

            HitData.DamageTypes damages = itemDrop.m_itemData.m_shared.m_damages;
            damages.m_spirit = damages.m_fire;
            damages.m_fire = 0f;
            itemDrop.m_itemData.m_shared.m_damages = damages;

            HitData.DamageTypes damagesPerLevel = itemDrop.m_itemData.m_shared.m_damagesPerLevel;
            damagesPerLevel.m_spirit = damagesPerLevel.m_fire;
            damagesPerLevel.m_fire = 0f;
            itemDrop.m_itemData.m_shared.m_damagesPerLevel = damagesPerLevel;

            Vector3 tipPosition = ReplaceFireLookWithWisp(item.transform);
            Transform handSocket = item.transform.Find("attach");
            if (handSocket != null)
            {
                tipPosition = handSocket.InverseTransformPoint(item.transform.TransformPoint(tipPosition));
            }
            else
            {
                handSocket = item.transform;
            }
            Vector3 orbPosition = tipPosition + new Vector3(-0.03f, 0.03f, 0f);
            CreatePrimitive(PrimitiveType.Sphere, handSocket, orbPosition, Vector3.one * 0.25f, GetDimmedWispMaterial());

            LODGroup lodGroup = item.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                Object.DestroyImmediate(lodGroup);
            }

            Sprite icon = Jotunn.Managers.RenderManager.Instance.Render(new Jotunn.Managers.RenderManager.RenderRequest(item)
            {
                Rotation = Jotunn.Managers.RenderManager.IsometricRotation
            });
            if (icon != null)
            {
                itemDrop.m_itemData.m_shared.m_icons = new[] { icon };
            }

            return item;
        }

        private static Vector3 ReplaceFireLookWithWisp(Transform root)
        {
            Vector3? tipLocalPosition = null;

            Light firstLight = root.GetComponentInChildren<Light>(true);
            if (firstLight != null)
            {
                tipLocalPosition = root.InverseTransformPoint(firstLight.transform.position);
            }

            foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps.transform != root)
                {
                    if (tipLocalPosition == null)
                    {
                        tipLocalPosition = root.InverseTransformPoint(ps.transform.position);
                    }
                    Object.DestroyImmediate(ps.gameObject);
                }
            }

            foreach (Light light in root.GetComponentsInChildren<Light>(true))
            {
                light.color = WispColor;
                light.range = Mathf.Min(light.range, 1.2f);
                light.intensity = Mathf.Min(light.intensity, 0.4f);
            }

            Material wispMaterial = GetDimmedWispMaterial();
            if (wispMaterial != null)
            {
                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    Material[] materials = renderer.sharedMaterials;
                    if (materials.Length >= 2)
                    {
                        materials[materials.Length - 1] = wispMaterial;
                        renderer.sharedMaterials = materials;
                    }
                    else if (materials.Length == 1)
                    {
                        renderer.sharedMaterial = wispMaterial;
                    }
                }
            }

            return tipLocalPosition ?? Vector3.zero;
        }

        private static GameObject CreatePrimitive(PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            if (material != null)
            {
                go.GetComponent<MeshRenderer>().sharedMaterial = material;
            }
            return go;
        }

        private static void AttachDemisterOrb(Transform parent, Vector3 localPosition, float scale, bool keepMistClearing)
        {
            GameObject source = PrefabManager.Instance.GetPrefab("demister_ball");
            if (source == null)
            {
                return;
            }

            GameObject orb = Object.Instantiate(source, parent);
            orb.name = "wisp_orb";
            orb.transform.localPosition = localPosition;
            orb.transform.localRotation = Quaternion.identity;
            orb.transform.localScale = Vector3.one * scale;

            foreach (ParticleSystem ps in orb.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = ps.main;
                main.loop = true;
                ps.Play();
            }

            foreach (ZSyncTransform syncTransform in orb.GetComponentsInChildren<ZSyncTransform>(true))
            {
                Object.DestroyImmediate(syncTransform);
            }
            foreach (ZNetView netView in orb.GetComponentsInChildren<ZNetView>(true))
            {
                Object.DestroyImmediate(netView);
            }

            if (!keepMistClearing)
            {
                foreach (Demister demister in orb.GetComponentsInChildren<Demister>(true))
                {
                    Object.DestroyImmediate(demister);
                }
                foreach (ParticleSystemForceField field in orb.GetComponentsInChildren<ParticleSystemForceField>(true))
                {
                    Object.DestroyImmediate(field);
                }
            }
        }

        private static Material GetDimmedWispMaterial()
        {
            Material source = GetWispMaterial();
            if (source == null)
            {
                return null;
            }

            Material copy = new Material(source);
            if (copy.HasProperty("_EmissionColor"))
            {
                copy.SetColor("_EmissionColor", WispColor * 0.6f);
            }
            if (copy.HasProperty("_Color"))
            {
                copy.SetColor("_Color", WispColor);
            }
            return copy;
        }

        private static Material GetWispMaterial()
        {
            MeshRenderer renderer = PrefabManager.Instance.GetPrefab("Wisp")?.GetComponentInChildren<MeshRenderer>(true);
            return renderer != null ? renderer.sharedMaterial : null;
        }
    }
}
