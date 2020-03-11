﻿//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of SAAB AB, or in
// accordance with the terms and conditions stipulated in the
// agreement/contract under which the program(s) have been
// supplied. 
//
//
// Information Class:	COMPANY UNCLASSIFIED
// Defence Secrecy:		NOT CLASSIFIED
// Export Control:		NOT EXPORT CONTROLLED
//
//
// File			: MapPos.cs
// Module		: Saab.Foundation.Map.Manager
// Description	: Definition of the MapPos map position structure
// Author		: Anders Modén		
//		
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************


using GizmoSDK.Coordinate;
using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;
using Saab.Utility.Map;
using Transform = Saab.Utility.Map.Transform;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;
using System;

namespace Saab.Foundation.Map
{
    public class MapPos : IMapLocation<Node>
    {
        public Node     node;                   // Local Context
        public Vec3D    position;               // Relative position to context
        public bool     clamped;                // true if this position is clamped
        public Vec3     normal;                 // Normal i local coordinate system
        public Matrix3  local_orientation;      // East North Up

        public Node Context { get { return node; } }
        public Vector3 Position
        {
            get
            {
                return new Vector3((float)position.x, (float)position.y, (float)position.z);
            }
        }

        protected Vec3 _euler; // PYR
        public Quaternion Rotation
        {
            get { return Quaternion.CreateFromYawPitchRoll(_euler.y, _euler.x, _euler.z); }
        }


        public void SetLatPos(double lat, double lon, double alt)
        {
            var mapControl = MapControl.SystemMap;
            if (mapControl == null)
            {
                return;
            }

            mapControl.SetPosition(this, new LatPos(lat,lon, alt));
        }

        public void SetCartPos(double x, double y, double z)
        {
            var mapControl = MapControl.SystemMap;
            if (mapControl == null)
            {
                return;
            }

            mapControl.SetPosition(this, new CartPos(x, y, z));
        }

        public void SetRotation(float yaw, float pitch, float roll)
        {
            _euler = new Vec3(pitch, yaw, roll);
        }

        virtual public Transform Step(double time, LocationOptions options)
        {
            Clamp(options);


            var up = new Vector3(normal.x, normal.y, -normal.z);


            Vector3 east = new Vector3(local_orientation.v11, local_orientation.v21, local_orientation.v31); ;
            Vector3 north;

            if (options.RotationOptions.HasFlag(RotationOptions.AlignToSurface))
            {
                east = Vector3.Normalize(east - (Vector3.Dot(east, up) * up));
                north = Vector3.Cross(east, up);
            }
            else
            {
                north = new Vector3(-local_orientation.v12, -local_orientation.v22, -local_orientation.v32);
            }

            var m = new Matrix4x4(east.X, east.Y, east.Z, 0,
                                    up.X, up.Y, up.Z, 0,
                                    north.X, north.Y, north.Z, 0,
                                    0, 0, 0, 1);

            var qm = Quaternion.CreateFromRotationMatrix(m);

            var r = qm * Rotation;

            return new Transform
            {
                Pos = { X = (float)position.x, Y = (float)position.y, Z = (float)position.z },
                Rot = r
            };
        }

        protected bool Clamp(LocationOptions options)
        {
            var mapControl = MapControl.SystemMap;
            if (mapControl == null)
            {
                return false;
            }

            var clampType = GroundClampType.NONE;
            if (options.PositionOptions == PositionOptions.Surface)
            {
                if (options.RotationOptions.HasFlag(RotationOptions.AlignToSurface))
                {
                    clampType = GroundClampType.GROUND_NORMAL_TO_SURFACE;
                }
                else
                {
                    clampType = GroundClampType.GROUND;
                }
            }

            var clampFlags = ClampFlags.DEFAULT;
            if (options.LoadOptions == LoadOptions.Load)
                clampFlags = ClampFlags.WAIT_FOR_DATA;

            if (options.QualityOptions == QualityOptions.Highest)
                clampFlags |= ClampFlags.ISECT_LOD_QUALITY;

            mapControl.UpdatePosition(this, clampType, clampFlags);

            return true;
        }


    }

    public class DynamicMapPos : MapPos, IDynamicLocation<Node>
    {
        private Vec3 _pos;
        private Vec3 _vel;
        private Vec3 _acc;
        private double _time;

        public void SetKinematicParams(double posX, double posY, double posZ, Vector3 vel, Vector3 acc, double t)
        {
            SetCartPos(posX, posY, posZ);
            _pos = new Vec3((float)position.x, (float)position.y, (float)position.z);

            var conv = new Coordinate();
            Matrix3 orientMatrix;
            conv.GetOrientationMatrix(new CartPos(posX, posY, posZ)).Inverse(out orientMatrix); // TODO, get Transpose instead

            
            _vel = new Vec3(vel.X, vel.Y, vel.Z);
            _acc = new Vec3(acc.X, acc.Y, acc.Z);
            _time = t;

            _vel = local_orientation * orientMatrix * _vel;
            _acc = local_orientation * orientMatrix * _acc;

        }

        public override Transform Step(double time, LocationOptions options)
        {   
            var dt = (float)(time - _time);
            var v = _vel * dt + 0.5f * _acc * dt * dt;

            if(Vec3.Dot(v, v) == 0.0 )
            {
                return base.Step(time, options);
            }

            position =  _pos + v;

            if (!options.RotationOptions.HasFlag(RotationOptions.AlignToVelocity))
                return base.Step(time, options);

            v = _vel + _acc * dt;
            v.Normalize();

            var n = new Vector3(local_orientation.v13, local_orientation.v23, local_orientation.v33);
            var u = new Vector3(v.x, v.y, v.z);

            var proj = u - Vector3.Dot(u, n) * n;

            var north = new Vector3(local_orientation.v12, local_orientation.v22, local_orientation.v32);
            var dot = Vector3.Dot(proj, north);
            var det = Vector3.Dot(n, Vector3.Cross(north,proj));

            _euler.y = -(float)Math.Atan2(det,dot);

            return base.Step(time, options);
        }
    }

}
