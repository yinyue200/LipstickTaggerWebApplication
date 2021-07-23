using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LipstickTaggerWebApplication.Data;
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
    public class WorkDetailModel : PageModel
    {
        public WorkDetailModel(ApplicationDbContext dbContext)
        {
            _applicationDbContext = dbContext;
        }
        private ApplicationDbContext _applicationDbContext;
        public bool AutoSave { get; set; }
        public string Path { get; set; }
        public string ImgPath { get; set; }
        public void OnGet(string path)
        {
            var user = _applicationDbContext.Users.Where(a => a.Id == User.Identity.Name).FirstOrDefault();
            var userSetting = _applicationDbContext.UserSettingEntities.Where(a => a.UserId == User.Identity.Name).FirstOrDefault();
            if(userSetting!=null)
                AutoSave = userSetting.EnableAutoSave;
            Tags = GetTagList().Select(a => new TagState(a, true)).ToList();
            //SelectedTags =  new string[] { "�޹�ͼƬ" };
        }
        public void OnPost(string path)
        {
            ;
        }
        [BindProperty]
        public List<TagState> Tags { get; set; }
        [BindProperty]
        public string[] SelectedTags { get; set; }
        public SelectList TagsSelect { get; set; }

        public static IEnumerable<string> GetTagList()
        {
            yield return "�޹�ͼƬ";
            yield return "���ں�ͿĨ����";
            yield return "���ں����";
            yield return "���ں챾��";
            yield return "���ں����װ";
        }
    }
}
