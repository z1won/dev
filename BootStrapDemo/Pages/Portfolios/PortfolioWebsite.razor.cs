using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BootStrapDemo.Pages.Portfolios
{
    public partial class PortfolioWebsite
    {

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected override void OnInitialized()
        {
            
        }


        protected void GotoBlazorDotNet5()
        {
            NavigationManagerReference.NavigateTo("/Index");
        }

    }
}
