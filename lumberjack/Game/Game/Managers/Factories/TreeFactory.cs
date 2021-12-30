using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Game_Objects;
using Game.Game_Objects.Trees;

namespace Game.Managers.Factories
{
    class TreeFactory : PropFactory
    {
        public TreeFactory() { }

        public override Prop getProp(ContentManager content, PropXMLStruct xmlInput)
        {
            Tree tree = null;

            switch (xmlInput.id)
            {
                case GameIDList.Prop_SmallFullTree:
                    tree = new SmallFullTree(content);
                    break;
                case GameIDList.Prop_LargeFullTree:
                    tree = new LargeFullTree(content);
                    break;
                case GameIDList.Prop_SmallBush:
                    tree = new SmallBush(content);
                    break;
                case GameIDList.Prop_BigBush:
                    tree = new BigBush(content);
                    break;
                case GameIDList.Prop_SpikeyBush:
                    tree = new SpikeyBush(content);
                    break;
            }

            if (tree != null)
            {
                tree.Position = xmlInput.position;
                tree.Rotation = xmlInput.rotation;
                tree.Scale = xmlInput.scale;
                tree.PPosition = xmlInput.position;
                tree.PRotation = xmlInput.rotation;
                tree.PScale = xmlInput.scale;
            }

            return tree;
        }

        public Prop getPropOnTerrain(ContentManager content, WorldNode node, PropXMLStruct xmlInput)
        {
            Tree tree = null;

            switch (xmlInput.id)
            {
                case GameIDList.Prop_SmallFullTree:
                    tree = new SmallFullTree(content);
                    break;
            }

            tree.Position = xmlInput.position;
            tree.Rotation = xmlInput.rotation;
            tree.Scale = xmlInput.scale;
            tree.PPosition = xmlInput.position;
            tree.PRotation = xmlInput.rotation;
            tree.PScale = xmlInput.scale;
            float h = tree.PositionY;
            if (node.Terrain.HeightData.IsOnHeightMap(tree.Position))
                node.Terrain.HeightData.GetHeightAndNormal(tree.Position, out h);
            tree.PositionY = h;
            tree.PPositionY = h;

            return tree;
        }
    }
}
