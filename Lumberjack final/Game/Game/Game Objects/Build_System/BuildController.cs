using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;
using Game.Math_Physics;
using Game.Managers;
using Game.States;

#pragma warning disable 0649    //never assigned to (BuildControllerSetup)


namespace Game.Game_Objects.Build_System
{
    public enum MoveDirection
    {
        MD_NOMOVE,
        MD_RIGHT,
        MD_UP,
        MD_LEFT,
        MD_DOWN,
        MD_FORWARD,
        MD_BACKWARD,
    };

    struct BuildControllerSetup
    {
        public Point3 dimensions;
        public Vector3 origin;
        public Vector3 stride;
        public SpriteManager sMan;
        public TextureManager tMan;
    }

    class BuildController
    {
        ContentManager content; //ref
        StateManager stateManager; //ref

        public List<BuildableObject> primitiveObjects;
        public List<BuildableObject> currentObjects;
        public List<CompleteObject> builtObjects;

        public List<BuildableObject> removeThese;

        //Grid Parameters
        Point3 gridDimensions;
        Vector3 origin;
        Vector3 stride;

        //Indicator
        Point3 currentLocation;
        Vector3 currentPosition;
        StaticMesh indicator;

        Viewport view;

        Player player;
        public int blocksRequired = 0;
        public int planksRequired = 0;
        public int rodsRequired = 0;
        public int disksRequired = 0;

        Keys pressedKey = Keys.None;

        public BuildController(ContentManager content, BuildControllerSetup setup, StateManager stateMan)
        {
            this.content = content;
            this.stateManager = stateMan;

            //List contruction
            primitiveObjects = new List<BuildableObject>();
            currentObjects = new List<BuildableObject>();
            builtObjects = new List<CompleteObject>();
            removeThese = new List<BuildableObject>();

            this.gridDimensions = setup.dimensions;
            this.origin = setup.origin;
            this.stride = setup.stride;

            currentLocation = new Point3(0, 0, 0);
            indicator = new StaticMesh();
            indicator.Initialize(content, "models\\building\\buildIndicator", MyColors.TreeTopGreen);
            indicator.Scale = new Vector3(.5f);
            indicator.Position = this.origin;

            initializeBuildableComponents();

            GameState gs = stateMan.getState("game") as GameState;
            player = gs.getPlayer();
        }

        public Point3 getIndicatorLocation()
        {
            return currentLocation;
        }

        public Vector3 getStride()
        {
            return stride;
        }

        public void initializeBuildableComponents()
        {
            BuildableObject temp = new BuildableObject(content.Load<Model>("models\\building\\block_centered"), 
                "models\\building\\block_centered", new Vector3(-20.0f, 0.0f, 0.0f), new Point3(0, 0, 0), PrimitiveType.Block);
            temp.type = PrimitiveType.Block;
            primitiveObjects.Add(temp);

            temp = new BuildableObject(content.Load<Model>("models\\building\\plank_centered"), 
                "models\\building\\plank_centered", new Vector3(-20.0f, 0.0f, 10.0f), new Point3(0, 0, 0), PrimitiveType.Plank);
            temp.type = PrimitiveType.Plank;
            primitiveObjects.Add(temp);

            temp = new BuildableObject(content.Load<Model>("models\\building\\disc_centered"), 
                "models\\building\\disc_centered", new Vector3(-20.0f, 0.0f, 20.0f), new Point3(0, 0, 0), PrimitiveType.Disk);
            temp.type = PrimitiveType.Disk;
            primitiveObjects.Add(temp);

            temp = new BuildableObject(content.Load<Model>("models\\building\\rod_centered"),
                "models\\building\\rod_centered", new Vector3(-20.0f, 0.0f, 30.0f), new Point3(0, 0, 0), PrimitiveType.Rod, false);
            temp.type = PrimitiveType.Rod;
            primitiveObjects.Add(temp);
        }

        public void finalizeObject()
        {
            CompleteObject newObject = new CompleteObject(stride);

            foreach (BuildableObject b in currentObjects)
            {
                newObject.addObject(b);
            }

            currentObjects.Clear();

            GameState game = stateManager.getState("game") as GameState;
            Player player = game.getPlayer();
            BuildState build = stateManager.getState("build") as BuildState;

            if (build.EnvironmentObject != null)
            {
                switch (build.EnvironmentObject.ID)
                {
                    case GameIDList.LogicProp_RiverDam:
                        newObject.name = "Dam Fix";
                        break;
                    case GameIDList.LogicProp_BrokenHouse:
                        newObject.name = "House Fix";
                        break;
                }
            }

            if (player != null)
            {
                newObject.blocks = blocksRequired;
                newObject.planks = planksRequired;
                newObject.rods = rodsRequired;
                newObject.disks = disksRequired;
                player.AddObject(newObject);
            }
        }

        public void moveIndicator(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.MD_BACKWARD:
                    currentLocation.z++;
                    break;
                case MoveDirection.MD_FORWARD:
                    currentLocation.z--;
                    break;
                case MoveDirection.MD_RIGHT:
                    currentLocation.x++;
                    break;
                case MoveDirection.MD_LEFT:
                    currentLocation.x--;
                    break;
                case MoveDirection.MD_UP:
                    currentLocation.y++;
                    break;
                case MoveDirection.MD_DOWN:
                    currentLocation.y--;
                    break;

                case MoveDirection.MD_NOMOVE:
                default:
                    //Do Nothing
                    break;
            };

            if (currentLocation.x > gridDimensions.x)
                currentLocation.x = gridDimensions.x;

            if (currentLocation.x < 0)
                currentLocation.x = 0;

            if (currentLocation.y > gridDimensions.y)
                currentLocation.y = gridDimensions.y;

            if (currentLocation.y < 0)
                currentLocation.y = 0;

            if (currentLocation.z > gridDimensions.z)
                currentLocation.z = gridDimensions.z;

            if (currentLocation.z < 0)
                currentLocation.z = 0;

            currentPosition.X = currentLocation.x * stride.X;
            currentPosition.Y = currentLocation.y * stride.Y;
            currentPosition.Z = currentLocation.z * stride.Z;

            indicator.Position = currentPosition;
        }

        #region Move API

        public void MoveUp()
        {
            moveIndicator(MoveDirection.MD_UP);
        }
        public void MoveDown()
        {
            moveIndicator(MoveDirection.MD_DOWN);
        }
        public void MoveLeft()
        {
            moveIndicator(MoveDirection.MD_LEFT);
        }
        public void MoveRight()
        {
            moveIndicator(MoveDirection.MD_RIGHT);
        }
        public void MoveForward()
        {
            moveIndicator(MoveDirection.MD_FORWARD);
        }
        public void MoveBackward()
        {
            moveIndicator(MoveDirection.MD_BACKWARD);
        }

        #endregion Move API

        #region Rotate API

        public void RotateForward()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                b.AddRotation = new Vector3(90.0f, 0.0f, 0.0f);
            }
        }

        public void RotateBackward()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                b.AddRotation = new Vector3(-90.0f, 0.0f, 0.0f);
            }
        }

        public void RotateLeft()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                b.AddRotation = new Vector3(0.0f, 90.0f, 0.0f);
            }
        }

        public void RotateRight()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                b.AddRotation = new Vector3(0.0f, -90.0f, 0.0f);
            }
        }

        #endregion Rotate API

        public void controlIndicatorWithKeys()
        {
            KeyboardState kb = Keyboard.GetState();

            //Move in the Z direction
            if (kb.IsKeyDown(Keys.Up))
            {
                pressedKey = Keys.Up;
            }
            else if (kb.IsKeyUp(Keys.Up) && pressedKey == Keys.Up)
            {
                pressedKey = Keys.None;
                moveIndicator(MoveDirection.MD_FORWARD);
            }
            if (kb.IsKeyDown(Keys.Down))
            {
                pressedKey = Keys.Down;
            }
            else if (kb.IsKeyUp(Keys.Down) && pressedKey == Keys.Down)
            {
                pressedKey = Keys.None;
                moveIndicator(MoveDirection.MD_BACKWARD);
            }
            

            //Move in the X direction
            if (kb.IsKeyDown(Keys.Left))
            {
                pressedKey = Keys.Left;
            }
            else if(kb.IsKeyUp(Keys.Left) && pressedKey == Keys.Left)
            {
                pressedKey = Keys.None;
                moveIndicator(MoveDirection.MD_LEFT);
            }
            if (kb.IsKeyDown(Keys.Right))
            {
                pressedKey = Keys.Right;
            }
            else if(kb.IsKeyUp(Keys.Right) && pressedKey == Keys.Right)
            {
                pressedKey = Keys.None;
                moveIndicator(MoveDirection.MD_RIGHT);
            }

            //Move in the y direction
            if (kb.IsKeyDown(Keys.NumPad8))
            {
                pressedKey = Keys.NumPad8;
            }
            else if(kb.IsKeyUp(Keys.NumPad8) && pressedKey == Keys.NumPad8)
            {
                pressedKey = Keys.None;
                moveIndicator(MoveDirection.MD_UP);
            }
            if (kb.IsKeyDown(Keys.NumPad2))
            {
                pressedKey = Keys.NumPad2;
            }
            else if(kb.IsKeyUp(Keys.NumPad2) && pressedKey == Keys.NumPad2)
            {
                pressedKey = Keys.None;
                moveIndicator(MoveDirection.MD_DOWN);
            }

            //Rotate around the Y
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                foreach (BuildableObject b in primitiveObjects)
                {
                    //b.rotate(new Vector3(0.0f, (float)Math.PI / 2.0f, 0.0f));
                    b.AddRotation = new Vector3(0.0f, 90.0f, 0.0f);
                }
            }

            //Rotate around the X
            if (Keyboard.GetState().IsKeyDown(Keys.RightControl))
            {
                foreach (BuildableObject b in primitiveObjects)
                {
                    //b.rotate(new Vector3((float)Math.PI / 2.0f, 0.0f, 0.0f));
                    b.AddRotation = new Vector3(90.0f, 0.0f, 0.0f);
                }
            }
        }

        public void createNewObjectOnGrid(BuildableObject creator)
        {
            bool canBeBuilt = true;

            BuildableObject newObj;
            if (creator.type == PrimitiveType.Rod)
            {
                newObj = new BuildableObject(currentPosition, currentLocation, creator, false);
            }
            else
            {

                newObj = new BuildableObject(currentPosition, currentLocation, creator);
            }

            foreach (BuildableObject b in currentObjects)
            {
                if (newObj.isInterceptedBy(b))
                {
                    canBeBuilt = false;
                }
            }

            if (canBeBuilt)
            {
                currentObjects.Add(newObj);
            }
        }

        public void BuildBlock()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                if (b.type == PrimitiveType.Block)
                {
                    createNewObjectOnGrid(b);
                }
            }
        }

        public void BuildPlank()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                if (b.type == PrimitiveType.Plank)
                {
                    createNewObjectOnGrid(b);
                }
            }
        }

        public void BuildRod()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                if (b.type == PrimitiveType.Rod)
                {
                    createNewObjectOnGrid(b);
                }
            }
        }

        public void BuildDisk()
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                if (b.type == PrimitiveType.Disk)
                {
                    createNewObjectOnGrid(b);
                }
            }
        }

        public void checkForClicked(BuildCamera cam, Viewport view)
        {
            foreach (BuildableObject b in primitiveObjects)
            {
                b.UpdateBoundingBox();
                if (b.isPicked(cam, view) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    createNewObjectOnGrid(b);
                }
            }
            removeThese.Clear();
            foreach (BuildableObject b in currentObjects)
            {
                if (b.isPicked(cam, view) && Mouse.GetState().RightButton == ButtonState.Pressed)
                {
                    removeThese.Add(b);
                }
            }

            foreach (BuildableObject b in removeThese)
            {
                currentObjects.Remove(b);
            }
        }

        public void update(GameTime time, BuildCamera cam)
        {
            controlIndicatorWithKeys();

            blocksRequired = 0;
            planksRequired = 0;
            rodsRequired = 0;
            disksRequired = 0;
            foreach (BuildableObject b in currentObjects)
            {
                switch (b.type)
                {
                    case PrimitiveType.Block:
                        blocksRequired++;
                        break;
                    case PrimitiveType.Disk:
                        disksRequired++;
                        break;
                    case PrimitiveType.Plank:
                        planksRequired++;
                        break;
                    case PrimitiveType.Rod:
                        rodsRequired++;
                        break;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                finalizeObject();
            }

            checkForClicked(cam, view);
        }

        public int getLiveObjectCount()
        {
            return currentObjects.Count;
        }

        public String GetPositionAsString()
        {
            return currentLocation.x + "," + currentLocation.y + "," + currentLocation.z;
        }

        public void render()
        {
            foreach (BuildableObject b in currentObjects)
            {
                b.Draw();
                if(Game1.drawDevelopment)
                    b.DrawOBB();
            }
            indicator.Draw();
        }

        public Viewport Viewport
        {
            get { return this.view; }
            set { this.view = value; }
        }
    }


}
