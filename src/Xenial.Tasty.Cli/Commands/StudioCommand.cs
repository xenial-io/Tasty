using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

namespace Xenial.Delicious.Cli.Commands
{
    public static class StudioCommand
    {
        private static FrameView? _leftPane;
        private static Toplevel? _top;
        public static Task<int> Studio(CancellationToken cancellationToken)
        {
            Application.Init();
            _top = Application.Top;

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Quit", "", () => Application.RequestStop() )
                }),
                new MenuBarItem ("_Color Scheme", CreateColorSchemeMenuItems()),
                new MenuBarItem ("_About...", "About Tasty.Cli", () =>  MessageBox.Query ("About Tasty.Cli", aboutMessage.ToString(), "_Ok")),
            });

            _leftPane = new FrameView("Commands")
            {
                X = 0,
                Y = 1, // for menu
                Width = 25,
                Height = Dim.Fill(1),
                CanFocus = false,
            };

            _top.Add(_leftPane, menu);

            SetColorScheme();

            Application.Run();
            return Task.FromResult(1);
        }

        static void SetColorScheme()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            _leftPane!.ColorScheme = _baseColorScheme;
#pragma warning restore CS8601 // Possible null reference assignment.
                              // _rightPane.ColorScheme = _baseColorScheme;
            _top?.SetNeedsDisplay();
        }

        static ColorScheme? _baseColorScheme;
        static MenuItem[] CreateColorSchemeMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            foreach (var sc in Colors.ColorSchemes)
            {
                var item = new MenuItem();
                item.Title = sc.Key;
                item.CheckType |= MenuItemCheckStyle.Radio;

#pragma warning disable CS8604 // Possible null reference argument.
                item.Checked = sc.Value == _baseColorScheme;
#pragma warning restore CS8604 // Possible null reference argument.

                item.Action += () =>
                {
                    _baseColorScheme = sc.Value;
                    SetColorScheme();
                    foreach (var menuItem in menuItems)
                    {
                        menuItem.Checked = menuItem.Title.Equals(sc.Key) && sc.Value == _baseColorScheme;
                    }
                };
                menuItems.Add(item);
            }
            return menuItems.ToArray();
        }

        private const string aboutMessage = @"
     Tasty
    delicious dotnet testing

                 .@@@@@@@@              
               @@@@%********%@@/        
             %@,     &@(/&@/,,,,(@@     
           @&******/%@@#.  %@%*,,,*&*   
        %@/.,*#&(,#*,,,,*(@@(//@(,,,(@, 
       .@(..........,%@/,,,,#@@%&@%*,*@#
       @(......(@#......(@/,,,%#  ,@/,#@
      @%......**.*........(@*,,#@.*@*,*@
     @%..........*@@&.......&#,,(%  &(,@
    @@............/#..*@@....%(,*@* (@@#
   %&//..............,*......*@/*&&@@   
  @@/////,....................@#%@#     
 (@(///////*................,%@/        
 @*..*////////,......,%@@@/             
@(......///////&@@@#                    
@@*....,%@@@&                           
";
    }
}
