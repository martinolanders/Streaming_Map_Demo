//******************************************************************************
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
// File			: Object.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzObject class
// Author		: Anders Mod�n		
// Product		: GizmoBase 2.10.5
//		
//
//			
// NOTE:	GizmoBase is a platform abstraction utility layer for C++. It contains 
//			design patterns and C++ solutions for the advanced programmer.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public interface INameInterface
        {
            string GetName();
            void SetName(string name);
        }

        public class Object : UserData
        {
            protected Object(IntPtr nativeReference) : base(nativeReference)
            {

            }
            public Object():base(Object_create())
            {

            }

            public static void InitializeFactory()
            {
                AddFactory(new Object());
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Object(nativeReference) as Reference;
            }

            public void SetAttribute(string userData, string name,DynamicType data)
            {
                Object_setAttribute(GetNativeReference(), userData, name,data.GetNativeReference());
            }
            public DynamicType GetAttribute(string userData,string name)
            {
                return new DynamicType(Object_getAttribute(GetNativeReference(), userData, name));
            }

            public bool HasAttribute(string userData, string name)
            {
                return Object_hasAttribute(GetNativeReference(), userData, name);
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Object_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Object_setAttribute(IntPtr object_reference, string userData, string name,IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Object_getAttribute(IntPtr object_reference,string userData,string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Object_hasAttribute(IntPtr object_reference, string userData, string name);


            #endregion
        }
    }
}
