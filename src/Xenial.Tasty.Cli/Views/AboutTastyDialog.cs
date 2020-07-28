using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Terminal.Gui;

using static Xenial.Delicious.Utils.PromiseHelper;

namespace Xenial.Delicious.Cli.Views
{
    public static class AboutTastyDialog
    {
        public static Task ShowDialogAsync()
            => Promise(resolve =>
            {
                MessageBox.Query("About Tasty.Cli", aboutMessage, "_Ok");
                resolve();
            });

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
