using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Game_Objects.Trees;
using Game.Game_Objects.Rocks;
using Game.Math_Physics;
using Game.States;

namespace Game.Managers
{
    /// <summary>Designed to encapsulate some quick and dirty functionality
    /// for some basic level editing</summary>
    public class LevelEditor
    {

        #region Member Variables

        ContentManager content;   //ref
        GraphicsDevice device;    //ref
        Camera camera;    //ref
        WorldManager world;     //ref
        SpriteFont font;      //ref
        GameState game;      //ref

        Point worldIndicies = new Point(-1, -1);

        bool grabObject = true;
        Prop prop;
        Trigger trigger;
        BillboardParticleEmitter emitter;
        LogicProp logicProp;

        Point3D point;

        bool showDirections = false;

        //input limiting
        int waitCounter = 0;
        const int waitUntil = 250;
        bool canExecute = true;

        //add to world limiting
        bool canAddToWorld = true;
        int addCount = 0;
        const int addMaxCount = 1000;

        //save word limiting
        bool canSave = true;
        int saveCount = 0;
        const int saveMaxCount = 1000;

        #endregion Member Variables


        #region Initialization

        /// <summary>Default ctor, must call initialize</summary>
        public LevelEditor() { }

        /// <summary>Initialize all pointers to main global objects</summary>
        public void Initialize(GameState game, ContentManager content, GraphicsDevice device, Camera camera)
        {
            this.content = content;
            this.device = device;
            this.camera = camera;
            this.game = game;
            world = WorldManager.getInstance();
            font = Renderer.getInstance().GameFont;

            point = new Point3D(device);
        }

        #endregion Initialization


        #region API

        /// <summary>hot keys and main key logic go in here</summary>
        public void Input(KeyboardState kb, MouseState ms)
        {
            if (kb.IsKeyDown(Keys.G) && kb.IsKeyDown(Keys.LeftShift))
                grabObject = false;

            if (kb.IsKeyDown(Keys.OemTilde))
                showDirections = true;
            else showDirections = false;

            if (kb.IsKeyDown(Keys.LeftAlt) && kb.IsKeyDown(Keys.Delete))
                killCurrent();

            if (canExecute)
            {
                if (kb.IsKeyDown(Keys.F1))
                    StartSmallFullTree();
                if (kb.IsKeyDown(Keys.F2))
                    StartLargeFullTree();
                if (kb.IsKeyDown(Keys.F3))
                    StartThinTree();
                if (kb.IsKeyDown(Keys.F4))
                    StartPineTree();
                if (kb.IsKeyDown(Keys.F5))
                    StartSmallBush();
                if (kb.IsKeyDown(Keys.F6))
                    StartLongRock();
                if (kb.IsKeyDown(Keys.F7))
                    StartBoulderRock();
                if (kb.IsKeyDown(Keys.F8))
                    StartCraigRock();
                if (kb.IsKeyDown(Keys.B))
                    StartWorldBorder();
                if (kb.IsKeyDown(Keys.T))
                    StartWorldTrigger();
                if (kb.IsKeyDown(Keys.P))
                    StartParticleEmitter();
                if (kb.IsKeyDown(Keys.D1))
                    StartRiverDam();
                if (kb.IsKeyDown(Keys.D2))
                    StartBrokenHouse();
                if (kb.IsKeyDown(Keys.D3))
                    StartBoatDock();
                if (kb.IsKeyDown(Keys.D4))
                    StartFence();
                if (kb.IsKeyDown(Keys.D5))
                    StartBoatA();
                if (kb.IsKeyDown(Keys.D6))
                    StartBoatB();
                if (kb.IsKeyDown(Keys.D7))
                    StartCabbage();
            }

            if (grabObject == false &&
                (prop != null || trigger != null || emitter != null || logicProp != null))
            {
                TreeAdjustments(kb, ms);
                RockAdjustments(kb, ms);
                BorderAdjustments(kb, ms);
                TriggerAdjustments(kb, ms);
                EmitterAdjustments(kb, ms);
                LogicPropAdjustments(kb, ms);
            }

            if (prop == null && canExecute)
            {
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    Vector2 msPos = new Vector2(device.Viewport.Width / 2, device.Viewport.Height / 2);
                    Ray ray = Game1.GetMouseRay(Renderer.getInstance().OriginalViewport, this.camera);

                    List<Prop> propList = world.GetNodePropList(worldIndicies);

                    bool found = false;

                    foreach (Trigger trig in world.TriggerList)
                    {
                        OrientedBoundingBox obb = trig.Model.OBB;
                        float near, far;

                        switch (obb.Intersects(ray, out near, out far))
                        {
                            case -1: break;
                            default:
                                found = true;
                                trigger = trig;
                                canExecute = false;
                                break;
                        }
                        if (found)
                            break;
                    }

                    if (!found)
                    {
                        foreach (Prop prp in propList)
                        {
                            OrientedBoundingBox obb = prp.GetBoundingModel().OBB;
                            float near, far;
                            switch (obb.Intersects(ray, out near, out far))
                            {
                                case -1: break;
                                default:
                                    found = true;
                                    prop = prp;
                                    canExecute = false;
                                    break;
                            }
                            if (found)
                                break;
                        }
                    }
                }
            }

            if (canExecute)
            {
                if (kb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.LeftShift))
                {
                    if (prop != null)
                    {
                        world.addProp(prop);
                        canExecute = canAddToWorld = false;
                        prop = null;
                    }
                    else if (trigger != null)
                    {
                        trigger.Model.GenerateBoundingBox();
                        world.AddTrigger(trigger);
                        canAddToWorld = canExecute = false;
                        trigger = null;
                    }
                    else if (emitter != null)
                    {
                        world.AddEmitter(emitter);
                        canAddToWorld = canExecute = false;
                        emitter = null;
                    }
                    else if (logicProp != null)
                    {
                        game.AddLogicalProp(logicProp);
                        canExecute = canAddToWorld = false;
                        logicProp = null;
                    }
                }
            }

            if (canSave)
            {
                if (kb.IsKeyDown(Keys.F12) && kb.IsKeyDown(Keys.LeftShift))
                {
                    world.LevelEditorSave();
                    canSave = false;
                }
            }
        }

        private void killCurrent()
        {
            prop = null;
            emitter = null;
            trigger = null;
            logicProp = null;
            grabObject = true;
            canExecute = false;
        }

        /// <summary>Main gameTime update</summary>
        public void Update(GameTime gameTime)
        {
            point.Position = camera.Position + camera.RotationMatrix.Forward * 500;

            //update objects to be in front of camera
            if (prop != null && grabObject == true)
            {
                if (prop.ID > GameIDList.Trees_Begin && prop.ID < GameIDList.Trees_End)
                {
                    Tree t_tree = (Tree)prop;
                    t_tree.Position = point.Position;
                }
                else if (prop.ID > GameIDList.Rocks_Begin && prop.ID < GameIDList.Rocks_End)
                {
                    Rock t_rock = (Rock)prop;
                    t_rock.Position = point.Position;
                }
                else if (prop.ID == GameIDList.Prop_Border)
                {
                    WorldBorder t_border = (WorldBorder)prop;
                    t_border.Position = point.Position;
                }
            }
            else if (trigger != null && grabObject == true)
            {
                trigger.Model.Position = point.Position;
            }
            else if (emitter != null && grabObject == true)
            {
                emitter.Position = point.Position;
            }
            else if (logicProp != null && grabObject == true)
            {
                logicProp.Position = point.Position;
            }

            if (emitter != null)
            {
                emitter.Animate(gameTime);
                emitter.Update(gameTime);
            }

            //update world indicies
            float ignore;
            worldIndicies = world.getTerrainHeight(camera.Position, out ignore);

            updateExecuteTimer(gameTime);
            updateAddToWorldTimer(gameTime);
            updateSaveTimer(gameTime);

        }

        /// <summary>update save world timer</summary>
        private void updateSaveTimer(GameTime time)
        {
            if (canSave == false)
            {
                saveCount += time.ElapsedGameTime.Milliseconds;
                if (saveCount >= saveMaxCount)
                {
                    saveCount = 0;
                    canSave = true;
                }
            }
        }

        /// <summary>update main add to world timer</summary>
        private void updateAddToWorldTimer(GameTime time)
        {
            if (canAddToWorld == false)
            {
                addCount += time.ElapsedGameTime.Milliseconds;
                if (addCount >= addMaxCount)
                {
                    addCount = 0;
                    canAddToWorld = true;
                }
            }
        }

        /// <summary>update main execute timer</summary>
        private void updateExecuteTimer(GameTime time)
        {
            if (canExecute == false)
            {
                waitCounter += time.ElapsedGameTime.Milliseconds;
                if (waitCounter >= waitUntil)
                {
                    canExecute = true;
                    waitCounter = 0;
                }
            }
        }

        /// <summary>Draw the object we're currently working on</summary>
        public void Draw()
        {
            point.Draw();

            if (prop != null)
            {
                prop.Draw();
                prop.GetBoundingModel().UpdateBoundingBox();
                prop.GetBoundingModel().DrawOBB();
            }
            else if (trigger != null)
            {
                trigger.Draw();
                trigger.Model.UpdateBoundingBox();
                trigger.Model.DrawOBB();
            }
            else if (emitter != null)
            {
                emitter.Draw();
                emitter.UpdateOBB();
                emitter.DrawOBB();
            }
            else if (logicProp != null)
            {
                logicProp.Draw();
                logicProp.GetBoundingModel().UpdateBoundingBox();
                logicProp.GetBoundingModel().DrawOBB();
            }
        }

        /// <summary>Draw Controls to the screen</summary>
        public void DrawText(SpriteBatch batch)
        {
            float stride = 20f;
            Vector2 position = new Vector2(20, 20);

            batch.DrawString(font, "LEVEL EDITOR ACTIVE", new Vector2(device.Viewport.Width / 2 - 75, 50), Color.IndianRed);
            batch.DrawString(font, point.Position.ToString(), new Vector2(device.Viewport.Width / 2 - 100, 15), Color.AliceBlue);

            if (trigger != null)
            {
                batch.DrawString(font, "TRIGGER ID: " + trigger.ID, new Vector2(device.Viewport.Width / 2 - 75, 100), Color.AliceBlue);
            }

            if (showDirections == true)
            {
                batch.DrawString(font, ":Start New Object = F1-F8", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Let Go of Object = Shift + G", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Add into WorldList = Shift + Enter", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Save World Lists = Shift + F12", position, Color.FloralWhite);

                position.Y += stride * 2;
                batch.DrawString(font, ":Position.Y = Ctrl + Up/Down", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Position.Z = Alt + Up/Down", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Position.X = Alt + Left/Right", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Rotation.Y = Home/End", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Rotation.Z = Insert/Delete", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Rotation.X = PageUp/PageDown", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, ":Scale = n7/n8 - Z,X,C", position, Color.FloralWhite);

                position.Y += stride * 2;
                batch.DrawString(font, "Current World Node Indicies: (" + worldIndicies.X + ", " + worldIndicies.Y + ")", position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, "Node Prop Count:     " + world.GetNodePropCount(worldIndicies), position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, "Total Prop Count:    " + world.GetTotalPropCount(), position, Color.FloralWhite);

                position.Y += stride;
                batch.DrawString(font, "Total Trigger Count:    " + world.TriggerList.Count, position, Color.FloralWhite);

                position.Y += stride * 2;
                batch.DrawString(font, "Cam Position: " + world.Camera.Position.ToString(), position, Color.FloralWhite);
            }

            if (canAddToWorld == false)
            {
                position.Y += stride * 2;
                batch.DrawString(font, "OBJECT ADDED TO PROP LIST", position, Color.DarkRed);
            }

            if (canSave == false)
            {
                position.Y += stride * 2;
                batch.DrawString(font, "WORLD SAVE COMPLETED", position, Color.DarkRed);
            }
        }

        #endregion API


        #region Privates

        /// <summary> Make adjustments to a Tree type prop</summary>
        private void TreeAdjustments(KeyboardState kb, MouseState ms)
        {
            if (prop == null)
                return;

            Tree t_tree = null;
            if (prop.ID > GameIDList.Trees_Begin && prop.ID < GameIDList.Trees_End)
                t_tree = (Tree)prop;
            else
                return;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                t_tree.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                t_tree.PositionY -= 1f;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionZ -= 1f;

            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionX -= 1f;

            if (kb.IsKeyDown(Keys.Home))
                t_tree.RotationY += .5f;
            if (kb.IsKeyDown(Keys.End))
                t_tree.RotationY -= .5f;

            if (kb.IsKeyDown(Keys.Insert))
                t_tree.RotationZ += .5f;
            if (kb.IsKeyDown(Keys.Delete))
                t_tree.RotationZ -= .5f;

            if (kb.IsKeyDown(Keys.PageUp))
                t_tree.RotationX += .5f;
            if (kb.IsKeyDown(Keys.PageDown))
                t_tree.RotationX -= .5f;

            if (kb.IsKeyDown(Keys.NumPad7))
                t_tree.Scale += new Vector3(.01f);
            if (kb.IsKeyDown(Keys.NumPad8))
                t_tree.Scale -= new Vector3(.01f);

            if (kb.IsKeyDown(Keys.Back))
                t_tree.Rotation = Vector3.Zero;

        }

        /// <summary> Make adjustments to a rock type prop</summary>
        private void RockAdjustments(KeyboardState kb, MouseState ms)
        {
            if (prop == null)
                return;

            Rock t_rock = null;

            if (prop.ID > GameIDList.Rocks_Begin && prop.ID < GameIDList.Rocks_End)
                t_rock = (Rock)prop;
            else
                return;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                t_rock.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                t_rock.PositionY -= 1f;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionZ -= 1f;

            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionX -= 1f;

            if (kb.IsKeyDown(Keys.Home))
                t_rock.RotationY += .5f;
            if (kb.IsKeyDown(Keys.End))
                t_rock.RotationY -= .5f;

            if (kb.IsKeyDown(Keys.Insert))
                t_rock.RotationZ += .5f;
            if (kb.IsKeyDown(Keys.Delete))
                t_rock.RotationZ -= .5f;

            if (kb.IsKeyDown(Keys.PageUp))
                t_rock.RotationX += .5f;
            if (kb.IsKeyDown(Keys.PageDown))
                t_rock.RotationX -= .5f;

            if (kb.IsKeyDown(Keys.NumPad7))
                t_rock.Scale += new Vector3(.01f);
            if (kb.IsKeyDown(Keys.NumPad8))
                t_rock.Scale -= new Vector3(.01f);

            if (kb.IsKeyDown(Keys.Back))
                t_rock.Rotation = Vector3.Zero;
        }

        /// <summary>Make adjustments to a world border type prop</summary>
        private void BorderAdjustments(KeyboardState kb, MouseState ms)
        {
            WorldBorder border = null;

            if (prop == null)
                return;

            if (prop.ID == GameIDList.Prop_Border)
                border = (WorldBorder)prop;
            else
                return;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                border.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                border.PositionY -= 1f;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                border.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                border.PositionZ -= 1f;

            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                border.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                border.PositionX -= 1f;

            if (kb.IsKeyDown(Keys.Home))
                border.RotationY += .5f;
            if (kb.IsKeyDown(Keys.End))
                border.RotationY -= .5f;

            if (kb.IsKeyDown(Keys.Insert))
                border.RotationZ += .5f;
            if (kb.IsKeyDown(Keys.Delete))
                border.RotationZ -= .5f;

            if (kb.IsKeyDown(Keys.PageUp))
                border.RotationX += .5f;
            if (kb.IsKeyDown(Keys.PageDown))
                border.RotationX -= .5f;

            if (kb.IsKeyDown(Keys.NumPad7))
                border.Scale += new Vector3(.01f);
            if (kb.IsKeyDown(Keys.NumPad8))
                border.Scale -= new Vector3(.01f);

            if (kb.IsKeyDown(Keys.Z) && kb.IsKeyDown(Keys.LeftControl))
                border.ScaleX -= .05f;
            else if (kb.IsKeyDown(Keys.Z))
                border.ScaleX += .05f;

            if (kb.IsKeyDown(Keys.X) && kb.IsKeyDown(Keys.LeftControl))
                border.ScaleY -= .05f;
            else if (kb.IsKeyDown(Keys.X))
                border.ScaleY += .05f;

            if (kb.IsKeyDown(Keys.C) && kb.IsKeyDown(Keys.LeftControl))
                border.ScaleZ -= .05f;
            else if (kb.IsKeyDown(Keys.C))
                border.ScaleZ += .05f;

            if (kb.IsKeyDown(Keys.Back))
            {
                border.Rotation = Vector3.Zero;
                border.Scale = Vector3.One;
            }
        }

        /// <summary>Make adjustments to a trigger</summary>
        private void TriggerAdjustments(KeyboardState kb, MouseState ms)
        {
            if (trigger == null)
                return;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                trigger.Model.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                trigger.Model.PositionY -= 1f;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                trigger.Model.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                trigger.Model.PositionZ -= 1f;

            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                trigger.Model.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                trigger.Model.PositionX -= 1f;

            if (kb.IsKeyDown(Keys.Home))
                trigger.Model.RotationY += .5f;
            if (kb.IsKeyDown(Keys.End))
                trigger.Model.RotationY -= .5f;

            if (kb.IsKeyDown(Keys.Insert))
                trigger.Model.RotationZ += .5f;
            if (kb.IsKeyDown(Keys.Delete))
                trigger.Model.RotationZ -= .5f;

            if (kb.IsKeyDown(Keys.PageUp))
                trigger.Model.RotationX += .5f;
            if (kb.IsKeyDown(Keys.PageDown))
                trigger.Model.RotationX -= .5f;

            if (kb.IsKeyDown(Keys.NumPad7))
                trigger.Model.Scale += new Vector3(.01f);
            if (kb.IsKeyDown(Keys.NumPad8))
                trigger.Model.Scale -= new Vector3(.01f);

            if (kb.IsKeyDown(Keys.Z) && kb.IsKeyDown(Keys.LeftControl))
                trigger.Model.ScaleX -= 1f;
            else if (kb.IsKeyDown(Keys.Z))
                trigger.Model.ScaleX += 1f;

            if (kb.IsKeyDown(Keys.X) && kb.IsKeyDown(Keys.LeftControl))
                trigger.Model.ScaleY -= 1f;
            else if (kb.IsKeyDown(Keys.X))
                trigger.Model.ScaleY += 1f;

            if (kb.IsKeyDown(Keys.C) && kb.IsKeyDown(Keys.LeftControl))
                trigger.Model.ScaleZ -= 1f;
            else if (kb.IsKeyDown(Keys.C))
                trigger.Model.ScaleZ += 1f;

            if (kb.IsKeyDown(Keys.Back))
            {
                trigger.Model.Rotation = Vector3.Zero;
                trigger.Model.Scale = Vector3.Zero;
            }
        }

        /// <summary>make adjustments to the current particle emitter</summary>
        private void EmitterAdjustments(KeyboardState kb, MouseState ms)
        {
            if (emitter == null)
                return;

            //position
            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                emitter.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                emitter.PositionY -= 1f;
            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                emitter.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                emitter.PositionZ -= 1f;
            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                emitter.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                emitter.PositionX -= 1f;

            //size
            if (kb.IsKeyDown(Keys.Z) && kb.IsKeyDown(Keys.LeftControl))
                emitter.Parameters.SpawnRadius.X -= 1f;
            else if (kb.IsKeyDown(Keys.Z))
                emitter.Parameters.SpawnRadius.X += 1f;
            if (kb.IsKeyDown(Keys.X) && kb.IsKeyDown(Keys.LeftControl))
                emitter.Parameters.SpawnRadius.Y -= 1f;
            else if (kb.IsKeyDown(Keys.X))
                emitter.Parameters.SpawnRadius.Y += 1f;
            if (kb.IsKeyDown(Keys.C) && kb.IsKeyDown(Keys.LeftControl))
                emitter.Parameters.SpawnRadius.Z -= 1f;
            else if (kb.IsKeyDown(Keys.C))
                emitter.Parameters.SpawnRadius.Z += 1f;
        }

        /// <summary>made adjustments to a logical prop</summary>
        private void LogicPropAdjustments(KeyboardState kb, MouseState ms)
        {
            if (logicProp == null)
                return;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                logicProp.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                logicProp.PositionY -= 1f;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                logicProp.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                logicProp.PositionZ -= 1f;

            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                logicProp.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                logicProp.PositionX -= 1f;

            if (kb.IsKeyDown(Keys.Home))
                logicProp.RotationY += .5f;
            if (kb.IsKeyDown(Keys.End))
                logicProp.RotationY -= .5f;

            if (kb.IsKeyDown(Keys.Insert))
                logicProp.RotationZ += .5f;
            if (kb.IsKeyDown(Keys.Delete))
                logicProp.RotationZ -= .5f;

            if (kb.IsKeyDown(Keys.PageUp))
                logicProp.RotationX += .5f;
            if (kb.IsKeyDown(Keys.PageDown))
                logicProp.RotationX -= .5f;

            if (kb.IsKeyDown(Keys.NumPad7))
                logicProp.Scale += new Vector3(.01f);
            if (kb.IsKeyDown(Keys.NumPad8))
                logicProp.Scale -= new Vector3(.01f);

            if (kb.IsKeyDown(Keys.Back))
            {
                logicProp.Rotation = Vector3.Zero;
                logicProp.Scale = Vector3.One;
            }
        }

        private void StartParticleEmitter()
        {

            if (emitter != null)
            {
                this.emitter = new BillboardParticleEmitter(device);
                this.emitter.Initialize(content, ParticleImage.PTL_IMG_WATER);
            }
            else
            {
                this.emitter = new BillboardParticleEmitter(device);
                this.emitter.Initialize(content, ParticleImage.PTL_IMG_LEAF);
            }
            grabObject = true;
            canExecute = false;
            trigger = null;
            prop = null;
            logicProp = null;
        }
        private void StartWorldBorder()
        {
            prop = new WorldBorder(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartSmallFullTree()
        {
            prop = new SmallFullTree(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartLargeFullTree()
        {
            prop = new LargeFullTree(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartPineTree()
        {
            prop = new PineTree(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartSmallBush()
        {
            prop = new SmallBush(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartBigBush()
        {
            prop = new BigBush(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartLongRock()
        {
            prop = new LongRock(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartBoulderRock()
        {
            prop = new BoulderRock(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartSpikeyBush()
        {
            prop = new SpikeyBush(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartCraigRock()
        {
            prop = new CraigRock(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartThinTree()
        {
            prop = new ThinTree(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            logicProp = null;
        }
        private void StartWorldTrigger()
        {
            trigger = new Trigger();
            trigger.Initialize(content);
            grabObject = true;
            canExecute = false;
            prop = null;
            emitter = null;
            logicProp = null;

            while (trigger.ID == -1)
            {
                int generatedID = Game1.random.Next(0, 1024);
                bool collisionFound = false;
                foreach (Trigger trig in world.TriggerList)
                {
                    if (trig.ID == generatedID)
                        collisionFound = true;
                }
                if (collisionFound == false)
                    trigger.ID = generatedID;
            }
        }
        private void StartRiverDam()
        {
            logicProp = new RiverDam(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            prop = null;
        }
        private void StartBrokenHouse()
        {
            logicProp = new BrokenHouse(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            prop = null;
        }
        private void StartBoatDock()
        {
            logicProp = new BoatDock(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            prop = null;
        }
        private void StartFence()
        {
            logicProp = new Fence(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            prop = null;
        }
        private void StartBoatA()
        {
            logicProp = new BoatA(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            prop = null;
        }
        private void StartBoatB()
        {
            logicProp = new BoatB(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            prop = null;
        }
        private void StartCabbage()
        {
            logicProp = new Cabbage(content);
            grabObject = true;
            canExecute = false;
            trigger = null;
            emitter = null;
            prop = null;
        }

        #endregion Privates


        #region Mutators

        public SpriteFont Font
        {
            get { return font; }
        }

        #endregion Mutators
    }
}
