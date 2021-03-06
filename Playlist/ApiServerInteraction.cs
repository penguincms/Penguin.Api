﻿using Penguin.Api.Abstractions.Interfaces;

namespace Penguin.Api.Playlist
{
    public class ApiServerInteraction<TRequest, TResponse> : IApiServerInteraction<TRequest, TResponse> where TRequest : IApiPayload where TResponse : IApiServerResponse
    {
        public string Id { get; set; }
        public TRequest Request { get; set; }
        IApiPayload IApiServerInteraction.Request => this.Request;
        public TResponse Response { get; set; }
        IApiServerResponse IApiServerInteraction.Response => this.Response;
    }

    public class ApiServerInteraction : IApiServerInteraction
    {
        public string Id { get; set; }
        public IApiPayload Request { get; set; }
        public IApiServerResponse Response { get; set; }

        public static ApiServerInteraction<TRequest, TResponse> Create<TRequest, TResponse>(TRequest request, TResponse response, string id) where TRequest : IApiPayload where TResponse : IApiServerResponse
        {
            return new ApiServerInteraction<TRequest, TResponse>()
            {
                Request = request,
                Response = response,
                Id = id
            };
        }
    }
}