using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BootStrapDemo.Pages
{
    // razor파일의 코드비하인드 파일은 ComponentBase를 기본적으로 상속받는다.
    // 그러므로 :ComponentBase는 생략가능
    public partial class About : ComponentBase
    {
        private readonly string Title = "정보(About) Page";
        private string SubTitle = "사이트 정보";
        protected override void OnInitialized()
        {
            SubTitle = DateTime.Now.ToLongTimeString();
        }


    }
}
