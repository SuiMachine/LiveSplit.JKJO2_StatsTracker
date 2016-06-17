﻿using LiveSplit.UI.Components;
using System;

namespace LiveSplit.MemoryGraph
{
    public class Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "JKJO2_Tracker"; }
        }
        public ComponentCategory Category
        {
            get { return ComponentCategory.Information; }
        }
        public string Description
        {
            get { return "Shows Jedi Knight 2 Jedi Outcast Stats"; }
        }
        public IComponent Create(Model.LiveSplitState state)
        {
            return new Component(state);
        }
        public string UpdateName
        {
            get { return ComponentName; }
        }
        public string UpdateURL
        {
            get { return ""; }
        }
        public Version Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public string XMLURL
        {
            get { return UpdateURL + "Components/update.LiveSplit.MemoryGraph.xml"; }
        }
    }
}