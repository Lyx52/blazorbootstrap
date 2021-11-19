﻿using BlazorBootstrap.States;
using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class Offcanvas : BaseComponent, IDisposable
    {
        #region Members

        private OffcanvasState state = new OffcanvasState { Visible = false };

        #endregion Members

        #region Methods

        protected override void OnAfterRender(bool firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            base.OnAfterRender(firstRender);
        }

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Offcanvas());
            builder.Append(BootstrapClassProvider.Offcanvas(Placement));

            base.BuildClasses(builder);
        }

        protected override void BuildStyles(StyleBuilder builder)
        {
            base.BuildStyles(builder);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public async Task ShowAsync()
        {
            await JS.InvokeVoidAsync("blazorBootstrap.offcanvas.show", ElementId, objRef);
        }

        public async Task HideAsync()
        {
            await JS.InvokeVoidAsync("blazorBootstrap.offcanvas.hide", ElementId);
        }

        [JSInvokable] public async Task bsShowOffcanvas() => await Showing.InvokeAsync();
        [JSInvokable] public async Task bsShownOffcanvas() => await Shown.InvokeAsync();
        [JSInvokable] public async Task bsHideOffcanvas() => await Hiding.InvokeAsync();
        [JSInvokable] public async Task bsHiddenOffcanvas() => await Hidden.InvokeAsync();

        public void Dispose()
        {
            objRef?.Dispose();
        }

        #endregion Methods

        #region Properties

        protected internal bool IsVisible => State.Visible == true;

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        [Parameter] public Placement Placement { get; set; } = Placement.End; // default

        protected internal OffcanvasState State => state;

        [Parameter] public bool Visible { get; set; }

        /// <summary>
        /// This event fires immediately when the show instance method is called.
        /// </summary>
        [Parameter] public EventCallback Showing { get; set; }

        /// <summary>
        /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).
        /// </summary>
        [Parameter] public EventCallback Shown { get; set; }

        /// <summary>
        /// This event is fired immediately when the hide method has been called.
        /// </summary>
        [Parameter] public EventCallback Hiding { get; set; }

        /// <summary>
        /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).
        /// </summary>
        [Parameter] public EventCallback Hidden { get; set; }

        /// <summary>
        /// Specifies the backdrop needs to be rendered for this.
        /// </summary>
        [Parameter] public bool ShowBackdrop { get; set; } = true;

        /// <summary>
        /// Specifies the content to be rendered inside this.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion Properties

        [Inject] OffcanvasService OffcanvasService { get; set; }

        [Inject] IJSRuntime JS { get; set; }

        private DotNetObjectReference<Offcanvas> objRef;

        private bool isShown = false; // default
    }
}
