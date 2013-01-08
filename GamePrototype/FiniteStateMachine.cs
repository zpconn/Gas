using System;
using System.Collections.Generic;
using Gas.Helpers;

namespace GamePrototype
{
    public abstract class FSMState
    {
        #region Variables
        protected int type = 0;
        #endregion

        #region Properties
        public int Type
        {
            get
            {
                return type;
            }
        }
        #endregion

        #region Abstract interface
        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update( float moveFactor );
        public abstract void Render( float moveFactor );
        public abstract int CheckTransitions( float moveFactor );
        #endregion
    }

    public class FiniteStateMachine
    {
        #region Variables
        private List<FSMState> states = new List<FSMState>();
        private FSMState currentState = null;
        private FSMState defaultState = null;
        private FSMState goalState = null;
        private int goalID = 0;
        #endregion

        #region Properties
        public FSMState DefaultState
        {
            set
            {
                if ( value == null )
                {
                    Log.Write( "Attempted to set a null FSMState as the default in a FiniteStateMachine." );
                    return;
                }

                defaultState = value;
            }
        }

        public int DefaultStateID
        {
            set
            {
                foreach ( FSMState state in states )
                {
                    if ( state.Type == value )
                    {
                        defaultState = state;
                        return;
                    }
                }

                Log.Write( "Attempted to assign the default state to one which does not exist." );
            }
        }

        public int GoalID
        {
            set
            {
                goalID = value;
            }
        }
        #endregion

        #region Interface
        public void Update( float moveFactor )
        {
            if ( states.Count == 0 )
                return;

            if ( currentState == null )
            {
                currentState = defaultState;
                currentState.Enter();
            }
            if ( currentState == null )
                return;

            int oldStateID = currentState.Type;
            goalID = currentState.CheckTransitions( moveFactor );

            if ( goalID != oldStateID )
            {
                if ( TransitionState( goalID ) )
                {
                    currentState.Exit();
                    currentState = goalState;
                    currentState.Enter();
                }
            }

            currentState.Update( moveFactor );
        }

        public void Render( float moveFactor )
        {
            if ( states.Count == 0 )
                return;

            if ( currentState == null )
            {
                currentState = defaultState;
                currentState.Enter();
            }
            if ( currentState == null )
                return;

            currentState.Render( moveFactor );
        }

        public void AddState( FSMState state )
        {
            if ( state == null )
            {
                Log.Write( "Attempted to add a null FSMState to a FiniteStateMachine." );
                return;
            }

            states.Add( state );
        }
        #endregion

        #region Helpers
        private bool TransitionState( int goalStateID )
        {
            foreach ( FSMState state in states )
            {
                if ( state.Type == goalStateID )
                {
                    goalState = state;
                    return true;
                }
            }

            if ( goalStateID == defaultState.Type )
                goalState = defaultState;

            return false;
        }
        #endregion
    }
}