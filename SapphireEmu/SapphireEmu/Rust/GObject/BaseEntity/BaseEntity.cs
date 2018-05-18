using System.Collections.Generic;
using ProtoBuf;
using SapphireEmu.Data.Base.GObject;
using UnityEngine;

namespace SapphireEmu.Rust.GObject
{
    public partial class BaseEntity : BaseNetworkable
    {
        public E_EntityFlags EntityFlags = 0;

        private BaseEntity m_Parent = null;
        private uint m_BoneID = 0;
        private List<BaseEntity> m_Children = new List<BaseEntity>();
        
        public BaseEntity GetParent() => m_Parent;
        public List<BaseEntity> GetChildren() => m_Children;
        
        public void SetParent(BaseEntity entity, uint bone = 0)
        {
            var lastparent = GetParent();
            if (lastparent != null)
            {
                lastparent.m_Children.Remove(this);
                lastparent.OnChildRemoved(this);
            }

            m_Parent = entity;
            m_BoneID = bone;
            
            if (m_Parent != null)
            {
                m_Parent.m_Children.Add(this);
                m_Parent.OnChildAdded(this);
            }
        }

        public virtual void OnChildRemoved(BaseEntity entity)
        {
        }
        public virtual void OnChildAdded(BaseEntity entity)
        {
        }

        #region [Methods] [Set|Has] Flag
        public void SetFlag(E_EntityFlags f, bool b)
        {
            if (b)
            {
                if (this.HasFlag(f))
                {
                    return;
                }
                this.EntityFlags |= f;
            }
            else
            {
                if (!this.HasFlag(f))
                {
                    return;
                }
                this.EntityFlags &= ~f;
            }
        }
        
        public bool HasFlag(E_EntityFlags f)
        {
            return ((this.EntityFlags & f) == f);
        }
        #endregion

        public override void GetEntityProtobuf(Entity entity)
        {
            base.GetEntityProtobuf(entity);

            entity.baseEntity = new ProtoBuf.BaseEntity
            {
                flags = (int) this.EntityFlags,
                pos = this.Position,
                rot = this.Rotation,
                skinid = 0,
                time = 1f
            };
            
            if (m_Parent != null)
            {
                entity.parent = new ParentInfo
                {
                    uid = m_Parent.UID,
                    bone = m_BoneID
                };
            }
        }
    }
}