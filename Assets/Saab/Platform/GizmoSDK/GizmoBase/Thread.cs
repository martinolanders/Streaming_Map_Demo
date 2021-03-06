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
// File			: Thread.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzThread class
// Author		: Anders Mod�n		
// Product		: GizmoBase 2.10.6
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
// AMO	191206	Created file 	        (2.10.5)
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public interface IThread
        {
            bool IsRunning(bool tick=false);
            bool IsStopping(bool tick = false);

            bool Run(bool waitForRunning = false);
            void Stop(bool waitForStopping = false);
        }

        public class Thread
        {
            public static UInt32 GetThreadID()
            {
                return Thread_getThreadID();
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 Thread_getThreadID();

            #endregion
        }
    }
}

