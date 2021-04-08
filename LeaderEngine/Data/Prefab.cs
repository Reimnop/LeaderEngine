using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaderEngine
{
    public class Prefab
    {
        private List<Entity> prefabEntities = new List<Entity>();

        public void Instantiate()
        {
            while (prefabEntities.Count > 0)
                RecursivelySwitchCollection(prefabEntities[0]);
        }

        private void RecursivelySwitchCollection(Entity entity)
        {
            entity.SwitchCollection(DataManager.CurrentScene.SceneRootEntities);

            foreach (var en in entity.Children)
                RecursivelySwitchCollection(en);
        }

        public static Prefab FromEntityList(List<Entity> entities)
        {
            Prefab prefab = new Prefab();

            int min = entities.Min(x => FindDepth(x));
            var topEntities = entities.Where(x => FindDepth(x) == min);

            foreach (var entity in topEntities)
                entity.Parent = null;

            entities.ForEach(x => x.SwitchCollection(prefab.prefabEntities));

            return prefab;
        }

        private static int FindDepth(Entity entity)
        {
            int o = 0;
            while (entity.Parent != null)
            {
                entity = entity.Parent;
                o++;
            }
            return o;
        }
    }
}
