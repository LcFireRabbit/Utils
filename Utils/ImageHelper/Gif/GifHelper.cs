using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Utils.ImageHelper
{
    public static class GifHelper
    {
        /// <summary>
        /// 用Image播放GIF
        /// </summary>
        /// <param name="gif">GIF图片URI</param>
        /// <param name="gifPlaySeconds">GIF图片播放一次的秒数</param>
        /// <param name="imageControl">承载GIF的Image控件</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static bool GifToImageByAnimate(Uri gif, int gifPlaySeconds,Image imageControl,out string errorMsg)
        {
            Storyboard board = null;
            errorMsg = string.Empty;
            //确实是GIF
            if (!gif.IsFile || !gif.AbsolutePath.EndsWith(".gif"))
            {
                errorMsg = "Is not gif file!";
                return false;
            }
            try
            {
                List<BitmapFrame> frameList = new List<BitmapFrame>();
                //将gif解码
                GifBitmapDecoder decoder = new GifBitmapDecoder(gif
                    ,BitmapCreateOptions.PreservePixelFormat
                    ,BitmapCacheOption.Default);
                if (!Equals(decoder, null) && decoder.Frames != null)
                {
                    //将每一帧添加到列表
                    frameList.AddRange(decoder.Frames);
                    ObjectAnimationUsingKeyFrames objKeyAnimate = new ObjectAnimationUsingKeyFrames
                    {
                        Duration = new Duration(TimeSpan.FromSeconds(gifPlaySeconds))
                    };
                    foreach (var item in frameList)
                    {
                        DiscreteObjectKeyFrame k1_img1 = new DiscreteObjectKeyFrame(item);
                        objKeyAnimate.KeyFrames.Add(k1_img1);
                    }
                    //添加第一帧为图片的开始画面
                    imageControl.Source = frameList[0];
                    //循环动画
                    board = new Storyboard
                    {
                        RepeatBehavior = RepeatBehavior.Forever,
                        FillBehavior = FillBehavior.HoldEnd
                    };
                    board.Children.Add(objKeyAnimate);
                    //将动画设置给ImageControl
                    Storyboard.SetTarget(objKeyAnimate, imageControl);
                    Storyboard.SetTargetProperty(objKeyAnimate, new PropertyPath(nameof(Image.Source)));
                    //开始动画
                    board.Begin();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }
    }
}
