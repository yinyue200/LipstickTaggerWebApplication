using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LipstickTaggerWebApplication.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

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
    public record ReviewInfo(IEnumerable<string> ImgUrl, string RateContent, string RateSku, string Rater, string RateDate, string AppendContent, string AppendDate, IEnumerable<string> AppendImgUrl);
    class Alldata
    {
        public List<ReviewInfo> ReviewInfos { get; set; }
    }
    [Authorize]
    public class WorkDetailModel : PageModel
    {
        public WorkDetailModel(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, IMemoryCache memoryCache)
        {
            _applicationDbContext = dbContext;
            _userManager = userManager;
            _cache = memoryCache;
        }
        private UserManager<IdentityUser> _userManager;
        private ApplicationDbContext _applicationDbContext;
        [BindProperty]
        public bool AutoSave { get; set; }
        public string path { get; set; }
        public string ImgPath { get; set; }
        [BindProperty]
        public string TagCropResults { get; set; }
        public ReviewInfo InfoStr { get; set; }
        private IMemoryCache _cache;
        internal ReviewInfo GetInfo(string path)
        {
            static string hex(byte[] s)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(35);
                for (int i = 0; i < s.Length; i++)
                {
                    // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                    sb.Append(s[i].ToString("x2"));
                }
                return sb.ToString();
            }
            var dirpath = System.IO.Path.GetDirectoryName(path) + ".json";
            Dictionary<string, ReviewInfo> cacheinfo = null;
            if (_cache.TryGetValue(dirpath,out var jsonobj))
            {
                cacheinfo = (Dictionary<string, ReviewInfo>)jsonobj;
            }
            else
            {
                if(System.IO.File.Exists(dirpath))
                {
                    MD5 md5 = MD5.Create(); ;
                    Dictionary<string, ReviewInfo> keyValues = new Dictionary<string, ReviewInfo>();
                    var alldata = Newtonsoft.Json.JsonConvert.DeserializeObject<Alldata>(System.IO.File.ReadAllText(dirpath));
                    if (alldata == null)
                        return null;
                    foreach(var one in alldata.ReviewInfos)
                    {
                        foreach(var url in one.ImgUrl ?? Array.Empty<string>())
                        {
                            keyValues[hex(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url)))] = one;
                        }
                        foreach (var url in one.AppendImgUrl??Array.Empty<string>())
                        {
                            keyValues[hex(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url)))] = one;
                        }
                    }
                    _cache.Set(dirpath, keyValues);
                    cacheinfo = (Dictionary<string, ReviewInfo>)_cache.Get(dirpath);
                }
                else
                {
                    return null;
                }
            }
            var npath = System.IO.Path.GetFileNameWithoutExtension(path);
            var pi = npath.IndexOf('.');
            var nfilename = pi < 0 ? npath : npath.Substring(0, pi);
            cacheinfo.TryGetValue(nfilename, out var reviewInfo);
            return reviewInfo;
        }
        internal void geninfostr(string path)
        {
            InfoStr = GetInfo(path);
        }
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
            geninfostr(jsonpath);
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
            ImgPath = "/api/ApiData?path=" + System.Web.HttpUtility.UrlEncode(path);
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
            tagresult.TagCropResults =string.IsNullOrWhiteSpace(TagCropResults)?null:
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<TagCropResult>>(TagCropResults);
            var jsonpath = GetWorkJsonPath(path);
            await System.IO.File.WriteAllTextAsync(jsonpath,
                Newtonsoft.Json.JsonConvert.SerializeObject(tagresult));
            geninfostr(jsonpath);
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
