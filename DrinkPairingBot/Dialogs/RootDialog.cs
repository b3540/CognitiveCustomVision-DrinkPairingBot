﻿using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.CustomVision;
using System.Net.Http;

namespace DrinkPairingBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        public Task StartAsync(IDialogContext context)
        {
            // Return welcome message to user | デフォルトのメッセージをセット
            //context.PostAsync($"Hello, I'm Drink Pairing Bot!");
            context.PostAsync($"こんにちは！ドリンクおすすめ Botです。");

            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Variables declaration | 変数定義
            bool food = false;  // "food" tag                | "food" タグの有無
            string tag ="";     // food category tag         | 食べ物カテゴリータグ
            string msg = "";    // response message from bot | 返答メッセージ

            // Prep for Custom Vision API | Custom Vision API を使う準備
            var cvCred = new PredictionEndpointCredentials("9c631fd082474bffba4b981ff91ab7c0");
            var cvEp = new PredictionEndpoint(cvCred);
            var cvGuid = new Guid("8a9b8258-7161-42c9-8e93-a3faec5345f3");

            if (activity.Attachments?.Count != 0)
            {
                // Get attachment (photo) and get as Stream | 送られてきた画像を Stream として取得
                var photoUrl = activity.Attachments[0].ContentUrl;
                var client = new HttpClient();
                var photoStream = await client.GetStreamAsync(photoUrl);

                try
                {
                    // Predict Image using Custom Vision API | 画像を判定
                    var cvResult = await cvEp.PredictImageAsync(cvGuid, photoStream);


                    // Get food and category tag | food タグ および カテゴリーを取得
                    foreach (var item in cvResult.Predictions)
                    {
                        if (item.Probability > 0.8)
                        {
                            if (item.Tag == "food")
                            {
                                food = true;
                            }
                            else
                            {
                                tag = item.Tag;
                                break;
                            }
                        }
                    }

                }
                catch
                {
                    // Error Handling
                }


            }


            if (tag != "")
            {
                // Set message | タグに応じてメッセージをセット
                //msg = "This is " + tag + "! Looks good ;)";
                //msg = "この写真は " + tag + " だね♪";

                switch (tag)
                {
                    case "curry":
                        msg = "カレーおいしそう！甘いチャイでホッとしよう☕";
                        break;
                    case "gyoza":
                        msg = "やっぱ餃子にはビールだね🍺";
                        break;
                    case "pizza":
                        msg = "ピザには刺激的な炭酸飲料★はどうかな？";
                        break;
                    case "meat":
                        msg = "肉、にく、ニク♪ 赤ワインを合わせてどうぞ🍷";
                        break;
                    case "ramen":
                        msg = "やめられないよねー。ラーメンには緑茶でスッキリ☆";
                        break;
                    case "sushi":
                        msg = "今日はちょっとリッチにお寿司？合わせるなら日本酒かな🍶";
                        break;
                }
            }
            else if (food == true)
            {
                //msg = "I'm not sure what it is ...";
                msg = "この食べ物は分からないです．．．日本の夏は麦茶だね！";
            }
            else
            {
                //msg = "Send me food photo you are eating!";
                msg = "食べ物の写真を送ってね♪";
            }

            await context.PostAsync(msg);
            context.Wait(MessageReceivedAsync);
        }
    }
}