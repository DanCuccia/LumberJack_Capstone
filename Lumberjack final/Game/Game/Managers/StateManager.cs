using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Game_Objects;
using Game.Game_Objects.Build_System;
using Game.States;
using Game.Managers;

namespace Game
{
    public class StateManager
    {
        protected List<State> states;
        protected State currentState;
        protected Game1 thisGame;
        protected State overlapState;
        protected AudioManager audio;

        /// <summary>Default CTOR</summary>
        /// <param name="g">the game</param>
        public StateManager(Game1 g)
        {
            //Setting up default data for State Manager
            states = new List<State>();
            currentState = null;
            thisGame = g;
            audio = AudioManager.getInstance();

            //Add all states here
            addState(new BuildState(thisGame, this));
            addState(new MenuState(thisGame, this));
            addState(new GameState(thisGame, this));
            addState(new GameMenuState(thisGame, this));
            addState(new InventoryState(thisGame, this));
            addState(new MapState(thisGame, this));
            setCurrentState("menu");
        }

        /// <summary>dump all states</summary>
        public void empty()
        {
            states.Clear();
        }

        /// <summary>get the current state</summary>
        /// <returns>the current state, null or not</returns>
        public State getCurrentState()
        {
            return currentState;
        }

        /// <summary>Change the current state</summary>
        /// <param name="name">id of the new state to enter</param>
        /// <param name="unloadCurrent">whether or not to unload the current content</param>
        public void setCurrentState(String name, bool unloadCurrent = true)
        {
            if (overlapState != null)
                overlapState = null;

            for (int index = 0; index < states.Count; index++)
            {
                if (states[index].name == name)
                {
                    if (unloadCurrent)
                    {
                        close();
                    }

                    currentState = states[index];
                    if(! currentState.isInitialized)
                        currentState.initialize();

                    break;
                }
            }
        }

        /// <summary>Open a new state above the current state, updating and drawing both</summary>
        /// <param name="name">the id of the state to show</param>
        public void showOverlapState(String name)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].name == name)
                {
                    overlapState = states[i];
                    if (overlapState.isInitialized == false)
                        overlapState.initialize();
                }
            }
        }

        /// <summary>Close the overlapping state, if any</summary>
        /// <param name="unload">whether or not you want to unload overlap state's content</param>
        public void closeOverlapState(bool unload = true)
        {
            if (overlapState != null)
            {
                if (unload)
                    overlapState.close();
                overlapState = null;
            }
        }

        /// <summary>Add a new state in the master state list </summary>
        /// <param name="s">a contructed state</param>
        public void addState(State s)
        {
            if (s != null)
            {
                s.content = thisGame.Content;
                s.stateMan = this;
                states.Add(s);
            }
        }

        /// <summary>Retrieve a state by id name from the list</summary>
        /// <param name="name">id of the state</param>
        /// <returns>a constructed state out of the master state list</returns>
        public State getState(String name)
        {
            State temp = null;

            for (int index = 0; index < states.Count; index++)
            {
                if (states[index].name == name)
                {
                    temp = states[index];
                    break;
                }
            }

            return temp;
        }

        /// <summary>Total Game Shutdown</summary>
        public void forceClosed()
        {
            close();
            thisGame.Exit();
        }

        /// <summary>Init the current state</summary>
        public void initialize()
        {
            if (currentState != null &&
                currentState.isInitialized == false)
            {
                currentState.initialize();
                currentState.isInitialized = true;
            }
        }

        /// <summary> Main user-input call</summary>
        /// <param name="kb">current keyboard state</param>
        /// <param name="ms">current mouse state</param>
        public void input(KeyboardState kb, MouseState ms)
        {
            if (overlapState != null &&
                overlapState.isInitialized == true)
            {
                overlapState.input(kb, ms);
                return;
            }

            if (currentState != null &&
                currentState.isInitialized == true)
                currentState.input(kb, ms);
        }

        /// <summary>Main update call</summary>
        public void update(GameTime time)
        {
            audio.Update(time);

            if (overlapState != null &&
                overlapState.isInitialized == true)
                overlapState.update(time);
            
            if (currentState != null &&
                currentState.isInitialized == true)
                currentState.update(time);
        }

        /// <summary>Main 3D draw call</summary>
        public void render3D(GameTime time)
        {
            if (currentState != null &&
                currentState.isInitialized == true)
                if (overlapState != null)
                {
                    if (Renderer.m_renderPhase != RenderPhase.PHASE_DEPTH)
                        currentState.render3D(time);
                }
                else
                    currentState.render3D(time);

            if (overlapState != null &&
                overlapState.isInitialized == true)
            {
                overlapState.render3D(time);
            }
        }

        /// <summary>Main 2D draw call</summary>
        public void render2D(GameTime time, SpriteBatch batch)
        {
            if (currentState != null &&
                currentState.isInitialized == true)
                currentState.render2D(time, batch);

            if (overlapState != null &&
                overlapState.isInitialized == true)
                overlapState.render2D(time, batch);
        }

        /// <summary>unload current state's content</summary>
        public void close()
        {
            if (currentState != null &&
                currentState.isInitialized == true)
            {
                currentState.close();
                currentState.isInitialized = false;
            }
        }
    }
}
