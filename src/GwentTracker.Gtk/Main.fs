namespace GwentTracker.Gtk
    module Main =

        open System
        open Xamarin.Forms
        open Xamarin.Forms.Platform.GTK

        [<EntryPoint>]
        let Main(args) =
            Gtk.Application.Init()
            Forms.Init()

            let app = new GwentTracker.App()
            let window = new FormsWindow()
            window.LoadApplication(app)
            window.SetApplicationTitle("Hello Fabulous Linux")
            window.Show();

            Gtk.Application.Run()
            0
