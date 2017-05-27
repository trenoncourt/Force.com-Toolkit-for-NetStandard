using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Salesforce.Chatter.Models;
using Salesforce.Common;

namespace Salesforce.Chatter.FunctionalTests
{
    [TestClass]
    public class ChatterClientTests
    {
        public class BulkForceClientConfig
        {
            public string ConsumerKey { get; set; }

            public string ConsumerSecret { get; set; }

            public string UserName { get; set; }

            public string Password { get; set; }

            public string TokenRequestEndpointUrl { get; set; }
        }

        private AuthenticationClient _auth;
        private ChatterClient _chatterClient;

        [ClassInitialize]
        public void Init()
        {
            string jsonConfig = File.ReadAllText("appsettings.json");
            var config = JsonConvert.DeserializeObject<BulkForceClientConfig>(jsonConfig);

            _auth = new AuthenticationClient();
            _auth.UsernamePasswordAsync(config.ConsumerKey, config.ConsumerSecret, config.UserName, config.Password, config.TokenRequestEndpointUrl).Wait();
            
            _chatterClient = new ChatterClient(_auth.InstanceUrl, _auth.AccessToken, _auth.ApiVersion);
        }

        [TestMethod]
        public void Chatter_IsNotNull()
        {
            Assert.IsNotNull(_chatterClient);
        }

        [TestMethod]
        public async Task Chatter_Feeds_IsNotNull()
        {
            var feeds = await _chatterClient.FeedsAsync<object>();

            Assert.IsNotNull(feeds);
        }

        [TestMethod]
        public async Task Chatter_Users_Me_IsNotNull()
        {
            var me = await _chatterClient.MeAsync<UserDetail>();

            Assert.IsNotNull(me);
        }

        [TestMethod]
        public async Task Chatter_Users_Me_Id_IsNotNull()
        {
            var me = await _chatterClient.MeAsync<UserDetail>();

            Assert.IsNotNull(me.id);
        }

        [TestMethod]
        public async Task Chatter_PostFeedItem()
        {
            var feedItem = await postFeedItem(_chatterClient);
            Assert.IsNotNull(feedItem);
        }

        [TestMethod]
        public async Task Chatter_Add_Comment()
        {
            var feedItem = await postFeedItem(_chatterClient);
            var feedId = feedItem.Id;

            var messageSegment = new MessageSegmentInput
            {
                Text = "Comment testing 1, 2, 3",
                Type = "Text"
            };

            var body = new MessageBodyInput { MessageSegments = new List<MessageSegmentInput> { messageSegment } };
            var commentInput = new FeedItemInput
            {
                Attachment = null,
                Body = body
            };

            var comment = await _chatterClient.PostFeedItemCommentAsync<Comment>(commentInput, feedId);
            Assert.IsNotNull(comment);
        }

        [TestMethod]
        public async Task Chatter_Add_Comment_With_Mention_IsNotNull()
        {
            var feedItem = await postFeedItem(_chatterClient);
            var feedId = feedItem.Id;

            var me = await _chatterClient.MeAsync<UserDetail>();
            var meId = me.id;

            var messageSegment1 = new MessageSegmentInput
            {
                Id = meId,
                Type = "Mention",
            };

            var messageSegment2 = new MessageSegmentInput
            {
                Text = "Comment testing 1, 2, 3",
                Type = "Text",
            };

            var body = new MessageBodyInput
            {
                MessageSegments = new List<MessageSegmentInput>
                {
                    messageSegment1, 
                    messageSegment2
                }
            };
            var commentInput = new FeedItemInput
            {
                Attachment = null,
                Body = body
            };

            var comment = await _chatterClient.PostFeedItemCommentAsync<Comment>(commentInput, feedId);
            Assert.IsNotNull(comment);
        }

        [TestMethod]
        public async Task Chatter_Like_FeedItem_IsNotNull()
        {
            var feedItem = await postFeedItem(_chatterClient);
            var feedId = feedItem.Id;

            var liked = await _chatterClient.LikeFeedItemAsync<Like>(feedId);

            Assert.IsNotNull(liked);
        }

        [TestMethod]
        public async Task Chatter_Share_FeedItem_IsNotNull()
        {
            var feedItem = await postFeedItem(_chatterClient);
            var feedId = feedItem.Id;

            var me = await _chatterClient.MeAsync<UserDetail>();
            var meId = me.id;

            var sharedFeedItem = await _chatterClient.ShareFeedItemAsync<FeedItem>(feedId, meId);

            Assert.IsNotNull(sharedFeedItem);
        }

        [TestMethod]
        public async Task Chatter_Get_My_News_Feed_IsNotNull()
        {
            var myNewsFeeds = await _chatterClient.GetMyNewsFeedAsync<FeedItemPage>();

            Assert.IsNotNull(myNewsFeeds);
        }

        [TestMethod]
        public async Task Chatter_Get_My_News_Feed_WithQuery_IsNotNull()
        {
            var myNewsFeeds = await _chatterClient.GetMyNewsFeedAsync<FeedItemPage>("wade");

            Assert.IsNotNull(myNewsFeeds);
        }

        [TestMethod]
        public async Task Chatter_Get_Groups_IsNotNull()
        {
            var groups = await _chatterClient.GetGroupsAsync<GroupPage>();

            Assert.IsNotNull(groups);
        }
        
        [TestMethod]
        public async Task Chatter_Get_Group_News_Feed_IsNotNull()
        {
            var groups = await _chatterClient.GetGroupsAsync<GroupPage>();
            if (groups.Groups.Count > 0)
            {
                var groupId = groups.Groups[0].Id;
                var groupFeed = await _chatterClient.GetGroupFeedAsync<FeedItemPage>(groupId);

                Assert.IsNotNull(groupFeed);
            }
            else
            {
                Assert.AreEqual(0, groups.Groups.Count);
            }
        }

        [TestMethod]
        public async Task Chatter_Get_Topics_IsNotNull()
        {
            var topics = await _chatterClient.GetTopicsAsync<TopicCollection>();

            Assert.IsNotNull(topics);
        }

        [TestMethod]
        public async Task Chatter_Get_Users_IsNotNull()
        {
            var users = await _chatterClient.GetUsersAsync<UserPage>();

            Assert.IsNotNull(users);
        }

        #region private functions
        private async Task<FeedItem> postFeedItem(ChatterClient chatter)
        {
            var me = await chatter.MeAsync<UserDetail>();
            var id = me.id;

            var messageSegment = new MessageSegmentInput
            {
                Text = "Testing 1, 2, 3",
                Type = "Text"
            };

            var body = new MessageBodyInput { MessageSegments = new List<MessageSegmentInput> { messageSegment } };
            var feedItemInput = new FeedItemInput()
            {
                Attachment = null,
                Body = body,
                SubjectId = id,
                FeedElementType = "FeedItem"
            };

            var feedItem = await chatter.PostFeedItemAsync<FeedItem>(feedItemInput, id);
            return feedItem;
        }
        #endregion
    }
}
