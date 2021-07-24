using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LipstickTaggerWebApplication.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LipstickTaggerWebApplication.Pages
{
    public class TagState
    {
        public TagState()
        { }
        public TagState(string tag, bool enable)
        {
            Tag = tag;
            Enable = enable;
        }

        public string Tag { get; set; }
        public bool Enable { get; set; }
    }
    [Authorize]
    public class WorkDetailModel : PageModel
    {
        public WorkDetailModel(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _applicationDbContext = dbContext;
            _userManager = userManager;
        }
        private UserManager<IdentityUser> _userManager;
        private ApplicationDbContext _applicationDbContext;
        [BindProperty]
        public bool AutoSave { get; set; }
        public string path { get; set; }
        public string ImgPath { get; set; }
        [BindProperty]
        public string TagCropResults { get; set; }
        public async Task<IActionResult> OnGetAsync(string path)
        {
            this.path = path;
            var user = await _userManager.GetUserAsync(User);
            var userSetting = _applicationDbContext.UserSettingEntities.Where(a => a.UserId == user.Id).FirstOrDefault();
            if(userSetting!=null)
                AutoSave = userSetting.EnableAutoSave;
            Tags = GetTagList().Select(a => new TagState(a, false)).ToList();
            ImgPath = "/api/ApiData?path=" + System.Web.HttpUtility.UrlEncode(path);
            var jsonpath = GetWorkJsonPath(path);
            TagResult tagresult;
            if (System.IO.File.Exists(jsonpath))
            {
                tagresult = Newtonsoft.Json.JsonConvert.DeserializeObject<TagResult>(System.IO.File.ReadAllText(jsonpath));
                if(tagresult != null)
                {
                    foreach (var item in Tags)
                    {
                        item.Enable = tagresult.PhotosTags.Contains(item.Tag);
                    }
                    if(tagresult.TagCropResults!=null && tagresult.TagCropResults.Count>0)
                    {
                        TagCropResults = Newtonsoft.Json.JsonConvert.SerializeObject(tagresult.TagCropResults);
                    }
                }
            }
            //SelectedTags =  new string[] { "无关图片" };
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string action)
        {
            return Page();
        }
        public static string GetWorkJsonPath(string path)
        {
            return "mdata" + System.IO.Path.DirectorySeparatorChar + "photos" + System.IO.Path.DirectorySeparatorChar + path + ".json";
        }
        public async Task<IActionResult> OnPostSaveAsync(string path)
        {
            this.path = path;
            await SaveDataAsync(path);
            return Page();
        }

        private async Task SaveDataAsync(string path)
        {
            var user = await _userManager.GetUserAsync(User);
            var userSetting = _applicationDbContext.UserSettingEntities.Where(a => a.UserId == user.Id).FirstOrDefault();
            if (userSetting != null)
                userSetting.EnableAutoSave = AutoSave;
            else
                await _applicationDbContext.UserSettingEntities
                    .AddAsync(new UserSettingEntity() { UserId = user.Id, EnableAutoSave = AutoSave });
            await _applicationDbContext.SaveChangesAsync();
            var tagresult = new Data.TagResult();
            tagresult.Path = path;
            tagresult.LastEditBy = User.Identity.Name;
            tagresult.PhotosTags = Tags.Where(a => a.Enable).Select(a => a.Tag).ToList();
            tagresult.TagCropResults = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TagCropResult>>(TagCropResults);
            await System.IO.File.WriteAllTextAsync(GetWorkJsonPath(path),
                Newtonsoft.Json.JsonConvert.SerializeObject(tagresult));
        }

        public async Task<IActionResult> OnPostNextAsync(string path)
        {
            if(AutoSave)
            {
                await SaveDataAsync(path);
            }
            var worklist = await ViewWorksModel.getworklist(User.Identity.Name);
            var newpath = worklist[worklist.IndexOf(path) + 1];
            return RedirectToPage(new { path = newpath });
        }
        public async Task<IActionResult> OnPostPrevAsync(string path)
        {
            if (AutoSave)
            {
                await SaveDataAsync(path);
            }
            var worklist = await ViewWorksModel.getworklist(User.Identity.Name);
            var newpath = worklist[worklist.IndexOf(path) - 1];
            return RedirectToPage(new { path = newpath });
        }
        [BindProperty]
        public List<TagState> Tags { get; set; }

        public static IEnumerable<string> GetTagList()
        {
            yield return "无关图片";
            yield return "含口红涂抹样例";
            yield return "含口红膏体";
            yield return "含口红本体";
            yield return "含口红外包装";
        }
    }
}
