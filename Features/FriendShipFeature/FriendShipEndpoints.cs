using PingPong.API.Features.FriendShipFeature.AcceptFriendShipRequest;
using PingPong.API.Features.FriendShipFeature.AddNewFriend;
using PingPong.API.Features.FriendShipFeature.BlockFriendShipRequest;
using PingPong.API.Features.FriendShipFeature.GetFriendsRequest;
using PingPong.API.Features.FriendShipFeature.GetMyFriendShipRequests;
using PingPong.API.Features.FriendShipFeature.RejectFriendShipRequest;
using PingPong.API.Features.FriendShipFeature.UnblockFriendShipRequest;
using PingPong.API.Features.FriendShipFeature.UnFriendRequest;

namespace PingPong.API.Features.FriendShipFeature
{
    public static class FriendShipEndpoints
    {
        public static void MapFriendShipEndpoints(this WebApplication app)
        {
            var friendsGroup = app.MapGroup("/friends")
                .WithTags("Friends")
                .RequireAuthorization();

            AddFriend.MapEndpoint(friendsGroup);
            GetFriendRequests.MapEndpoint(friendsGroup);
            AcceptFriendShip.MapEndpoint(friendsGroup);
            RejectFriendShip.MapEndpoint(friendsGroup);
            BlockFriendShip.MapEndpoint(friendsGroup);
            GetFriends.MapEndpoint(friendsGroup);
            UnblockFriend.MapEndpoint(friendsGroup);
            UnFriend.MapEndpoint(friendsGroup);
        }
    }
}