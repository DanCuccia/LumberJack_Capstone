using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Game_Objects;
using Game.States;

namespace Game.Managers.Factories
{
    class TriggerFactory
    {
        GameState game;
        
        /// <summary>Default CTOR</summary>
        /// <param name="gameState">Trigger callbacks are tied to the GameState</param>
        public TriggerFactory(GameState gameState) 
        {
            this.game = gameState;
        }

        public Trigger GetTrigger(ContentManager content, TriggerXmlMedium xmlTrigger)
        {
            Trigger output = new Trigger();
            output.Initialize(content);

            output.ID = xmlTrigger.id;
            output.HasTriggered = xmlTrigger.hasTriggered;
            output.Repeatable = xmlTrigger.repeatable;
            output.Model.Position = xmlTrigger.position;
            output.Model.Rotation = xmlTrigger.rotation;
            output.Model.Scale = xmlTrigger.scale;

            output.Model.GenerateBoundingBox();
            output.Model.UpdateBoundingBox();

            switch (xmlTrigger.id)
            {
                default:
                    output.TriggerCallback = null;
                    break;
                case 703:
                    output.TriggerCallback = game.tut1_callback;
                    break;
                case 714:
                    //output.TriggerCallback = game.tut0_callback;
                    break;
                case 393:
                    output.TriggerCallback = game.ravineFall_callback;
                    break;
                case 507:
                    output.TriggerCallback = game.goal_callback;
                    break;
                case 361:
                    output.TriggerCallback = game.tip0_callback;
                    break;
                case 843:
                    output.TriggerCallback = game.tip1_callback;
                    break;
                case 861:
                    output.TriggerCallback = game.tip2_callback;
                    break;
                case 89:
                    output.TriggerCallback = game.tip3_callback;
                    break;
                case 410:
                    output.TriggerCallback = game.tip4_callback;
                    break;
                case 490:
                    output.TriggerCallback = game.tut1_2_callback;
                    break;
                case 25:
                    output.TriggerCallback = game.guide0_callback;
                    break;
                case 680:
                    output.TriggerCallback = game.guide1_callback;
                    break;
                case 733:
                    output.TriggerCallback = game.guide2_callback;
                    break;
                case 323:
                    output.TriggerCallback = game.guide3_callback;
                    break;
                case 23:
                    output.TriggerCallback = game.getPlayer().blockTriggerCallback;
                    break;
            }

            return output;
        }
    }
}
