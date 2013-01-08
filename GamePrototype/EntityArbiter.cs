using System;
using System.Collections.Generic;
using Microsoft.DirectX;

namespace GamePrototype
{
    public class EntityArbiter
    {
        #region Variables
        private List<Entity> entities = new List<Entity>();
        private List<Entity> newEntities = new List<Entity>();
        #endregion

        #region Public interface
        public void AddEntity( Entity entity )
        {
            newEntities.Add( entity );
        }

        public void Update( float moveFactor )
        {
            List<Entity> deadEntities = new List<Entity>();

            entities.AddRange( newEntities );
            newEntities.Clear();

            foreach ( Entity entity in entities )
            {
                entity.Update( moveFactor );

                if ( !entity.Alive )
                    deadEntities.Add( entity );
            }

            foreach ( Entity entity in deadEntities )
            {
                entity.Kill();
                entities.Remove( entity );
            }

            for ( int i = 0; i < entities.Count; ++i )
            {
                Entity ent1 = entities[ i ];

                for ( int j = i + 1; j < entities.Count; ++j )
                {
                    Entity ent2 = entities[ j ];

                    if ( ent1.Mesh != null && ent2.Mesh != null )
                    {
                        // Perform a preliminary bounding circle test
                        float distance = Vector2.Length( ent2.Position - ent1.Position );
                        if ( distance <= ent1.Mesh.Radius + ent2.Mesh.Radius &&
                            ent1.Mesh.Intersects( ent2.Mesh, ent1.WorldTransform, ent2.WorldTransform ) )
                        {
                            ent1.OnCollision( ent2 );
                            ent2.OnCollision( ent1 );
                        }
                    }
                }
            }
        }

        public void Render( float moveFactor )
        {
            foreach ( Entity entity in entities )
            {
                entity.Render( moveFactor );
            }
        }
        #endregion
    }
}