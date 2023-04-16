namespace Twitter_Telegram.Domain.Config
{
    public static class TwitterHelper
    {
        public static string UserInfoByIdUrl => "https://api.twitter.com/1.1/users/show.json?user_id={0}";

        public static string UserInfoByUsernameUrl => "https://api.twitter.com/1.1/users/show.json?screen_name={0}";
        
        public static string UserFriendIdsByUsernameUrl => "https://api.twitter.com/1.1/friends/ids.json?screen_name={0}";

        public static string UserIFriendsByUsernameUrl => "https://api.twitter.com/1.1/friends/list.json?cursor=-1&screen_name={0}&skip_status=true&include_user_entities=false";
    }
}
