// Copyright (c) 2021 tuke productions. All rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SimTuning.Maui.UI.Components
{
    public partial class MenuOptionView : StackLayout
    {
        public ImageSource ImageSource
        {
            set
            {
                Icon.ImageSource = value;
            }
        }

        public string Text
        {
            set
            {
                Label.Text = value;
            }
        }

        public MenuOptionView()
        {
            InitializeComponent();
        }
    }
}