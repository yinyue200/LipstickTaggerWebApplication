using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public bool AutoSave { get; set; }
        public string Path { get; set; }
        public string ImgPath { get; set; }
        public void OnGet(string path)
        {
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
