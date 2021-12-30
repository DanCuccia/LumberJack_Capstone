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

        public override Prop getProp(ContentManager content, PropXMLStruct2 xmlInput)
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
                case GameIDList.Prop_SmallStump:
                    tree = new SmallTreeStump(content);
                    break;
                case GameIDList.Prop_BigStump:
                    tree = new BigTreeStump(content);
                    break;
                case GameIDList.Prop_ThinTree:
                    tree = new ThinTree(content);
                    break;
                case GameIDList.Prop_ThinTreeStump:
                    tree = new ThinTreeStump(content);
                    break;
                case GameIDList.Prop_PineTree:
                    tree = new PineTree(content);
                    break;
                case GameIDList.Prop_PineTreeStump:
                    tree = new PineTreeStump(content);
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

    }
}
