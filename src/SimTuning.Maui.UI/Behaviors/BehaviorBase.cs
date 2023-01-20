// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Maui.UI.Behaviors
{
    using System;
    using Microsoft.Maui;
    using Microsoft.Maui.Controls;

    /// <summary>
    /// BehaviorBase.
    /// </summary>
    /// <typeparam name="T">object.</typeparam>
    /// <seealso cref="Xamarin.Forms.Behavior{T}" />
    public class BehaviorBase<T> : Behavior<T> where T : BindableObject
    {
        public T AssociatedObject { get; private set; }

        protected override void OnAttachedTo(T bindable)
        {
            base.OnAttachedTo(bindable);
            AssociatedObject = bindable;

            if (bindable.BindingContext != null)
            {
                BindingContext = bindable.BindingContext;
            }

            bindable.BindingContextChanged += OnBindingContextChanged;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            BindingContext = AssociatedObject.BindingContext;
        }

        protected override void OnDetachingFrom(T bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.BindingContextChanged -= OnBindingContextChanged;
            AssociatedObject = null;
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            OnBindingContextChanged();
        }
    }
}