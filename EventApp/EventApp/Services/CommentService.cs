﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace EventApp.Services
{



    public class CommentService : CommentInterface<Comment>
    {


        ObservableCollection<ObservableCollection<Comment>> comments;
        Comment individualComment;
        ObservableCollection<Comment> commentGroup;

        public string ShowReplyVal;
        public string ShowDeleteVal;
        public CommentService()
        {

        }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }

        public async Task<IEnumerable<IEnumerable<Comment>>> GetHolidayCommentsAsync(bool forceRefresh = false, string holidayId = null, string user = null)
        {
            comments = new ObservableCollection<ObservableCollection<Comment>>();

            var values = new Dictionary<string, string>{
                   { "holiday", holidayId },
            };
            if (isLoggedIn)
                values["username"] = currentUser;

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            
            dynamic commentList = responseJSON.results;
            
            foreach (var thread in commentList)
            {
                commentGroup = new ObservableCollection<Comment>();
                foreach (var comment in thread)
                {
                    string TimeAgo = comment.time_since;
                    string commentUser = comment.user;
                    if (String.Equals(commentUser, currentUser, StringComparison.OrdinalIgnoreCase))
                    {
                        ShowReplyVal = "false";
                        ShowDeleteVal = "true";
                    }
                    else
                    {
                        ShowReplyVal = "true";
                        ShowDeleteVal = "false";
                    }

                    int padding = comment.depth;
                    Xamarin.Forms.Thickness paddingThickness = new
                        Xamarin.Forms.Thickness(Convert.ToDouble(
                            padding), 10, 10, 10
                            );

                    string voteStatus = comment.vote_status;
                    string UpVoteImage = Utils.GetUpVoteImage(voteStatus);
                    string DownVoteImage = Utils.GetDownVoteImage(voteStatus);
                    commentGroup.Add(new Comment()
                    {
                        Id = comment.id,
                        Content = comment.content,
                        HolidayId = comment.holiday,
                        UserName = comment.user,
                        TimeSince = TimeAgo,
                        ShowReply = ShowReplyVal,
                        ShowDelete = ShowDeleteVal,
                        Votes = comment.votes,
                        UpVoteStatus = UpVoteImage, 
                        DownVoteStatus = DownVoteImage,
                        Parent = comment.parent,
                        ThreadPadding = paddingThickness
                    });
                }

                comments.Insert(0, commentGroup);

            }

            return await Task.FromResult(comments);
        }

        public async Task VoteComment(string commentId, string vote)
        {
            var values = new Dictionary<string, string>{
                   { "comment", commentId },
                   { "username", currentUser },
                   { "vote", vote }
                };

            var content = new FormUrlEncodedContent(values);

            await App.globalClient.PostAsync(App.HolidailyHost +
                "/comments/" + commentId + "/", content);

        }

        public async Task<Comment> GetCommentById(string id)
        {
            var values = new Dictionary<string, string>{
                   { "id", id }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/"+id+"/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic commentJSON = responseJSON.results;
            
            try
            { 
                string TimeAgo = commentJSON.time_since;

                individualComment = new Comment() { Content = commentJSON.content, HolidayId = commentJSON.holiday_id, UserName = commentJSON.user, TimeSince = TimeAgo };
            }
            catch
            {
                individualComment = null;
            }


            return await Task.FromResult(individualComment);

        }



    }
}