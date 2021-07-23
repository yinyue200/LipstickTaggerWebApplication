using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;
using System.IO;

namespace LipstickTaggerWebApplication.Pages
{
    [Authorize]
    public class ViewWorksModel : PageModel
    {
        SignInManager<IdentityUser> signInManager;
        public ViewWorksModel(SignInManager<IdentityUser> signInManager)
        {
            this.signInManager = signInManager;
        }
        List<string> WorkList;
        public async Task<List<string>> getworklist(string username)
        {
            return (await System.IO.File.ReadAllLinesAsync("mdata\\users\\" + username.Replace("@", "&")+"\\worklist.txt")).ToList();
        }
        public IPagedList<string> PagedWorkList;
        public async Task<IActionResult> OnGetAsync(int? page)
        {
            if(signInManager.IsSignedIn(User))
            {
                WorkList = await getworklist(User.Identity.Name);
                var pageNumber = page ?? 1;
                PagedWorkList = WorkList.ToPagedList(pageNumber, 50);
                return Page();
            }
            return Unauthorized();
        }
        public bool HasResult(string path)
        {
            return false;
        }
    }
}
