﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Starship.Core.Extensions;
using Starship.Core.Security;
using Starship.Data.Configuration;

namespace Starship.Azure.Data {
    public class Account : CosmosDocument, IsSecurityContext {
        
        public Account() {
            Type = "account";
        }

        public bool HasGroup(string id) {
            return GetGroups().Contains(id);
        }

        public List<string> GetGroups() {
            if(Groups == null) {
                return new List<string>();
            }

            return Groups.ToList();
        }

        public void RemoveGroup(string id) {
            if(HasGroup(id)) {
                var groups = GetGroups();
                groups.Remove(id);
                Groups = groups.Where(group => group.Length > 0).ToList();
            }
        }

        public void AddGroup(string id) {
            var groups = GetGroups();
            groups.Add(id);
            Groups = groups.Where(group => group.Length > 0).ToList();
        }

        public bool IsGroupLeader() {
            if(string.IsNullOrEmpty(Role)) {
                return false;
            }

            return Role.ToLower() == "leader";
        }
        
        public bool IsCoordinator() {

            if(string.IsNullOrEmpty(Role)) {
                return false;
            }

            return Role.ToLower() == "coordinator";
        }

        public bool IsManager() {

            if(string.IsNullOrEmpty(Role)) {
                return false;
            }

            return Role.ToLower() == "manager" || IsGroupLeader();
        }

        public bool IsAdmin() {

            if(string.IsNullOrEmpty(Role)) {
                return false;
            }

            return Role.ToLower() == "admin";
        }

        public bool CanDelete(CosmosDocument entity) {

            if(entity.IsSystemType() || entity.Owner == GlobalDataSettings.SystemOwnerName) {
                return false;
            }

            if(IsAdmin()) {
                return true;
            }
            
            return entity.Owner == Id;
        }

        public bool CanUpdate(CosmosDocument entity, List<string> shares) {

            if(entity.Type == "account" && Id == entity.Id) {
                return true;
            }

            if(IsAdmin()) {
                return true;
            }
            
            return CanRead(entity, shares);
        }

        public bool CanRead(CosmosDocument entity, List<string> shares) {

            if(IsAdmin()) {
                return true;
            }
            
            var groups = GetGroups();

            if(entity.Owner == Id || shares.Contains(entity.Owner) || shares.Contains(entity.Id) || entity.Participants.Any(participant => participant.Id == Id)) {
                return true;
            }
            
            if(entity.Type == "account") {
                return entity.GetPropertyValue<List<string>>("groups").Any(group => groups.Contains(group));
            }

            return false;
        }
        
        public string GetName() {

            if(LastName.Contains("@") && FirstName.Contains("@")) {
                return FirstName;
            }

            return FirstName + " " + LastName;
        }
        
        /*public Account UpdateComponent<T>(Action<T> action) where T : new() {
            var component = GetComponent<T>();
            action(component);
            SetComponent(component);
            return this;
        }*/

        /*public T GetComponent<T>(string key) where T : new() {

            if(Components == null) {
                Components = new Dictionary<string, object>();
            }
            
            if(!Components.ContainsKey(key)) {
                Components.Add(key, new T());
            }

            if(!(Components[key] is T)) {
                Components[key] = Components[key].Clone<T>();
            }
            
            return (T) Components[key];
        }*/

        //public T GetComponent<T>() where T : new() {
        //    return GetComponent<T>(GetComponentKey(typeof(T)));
        //}

        /*public void SetComponent<T>(T component, string key) where T : new() {
            
            if(Components == null) {
                Components = new Dictionary<string, object>();
            }

            var components = Components.Clone<Dictionary<string, object>>();
            
            if(!components.ContainsKey(key)) {
                components.Add(key, component);
            }
            else {
                components[key] = component;
            }

            Components = components;
        }
        
        private string GetComponentKey(Type type) {
            return type.Name.Replace("Component", "").CamelCase();
        }*/

        [Secure, JsonProperty(PropertyName="email")]
        public string Email {
            get => Get<string>("email");
            set => Set("email", value);
        }

        [Secure, JsonProperty(PropertyName="changeEmail")]
        public string ChangeEmail {
            get => Get<string>("changeEmail");
            set => Set("changeEmail", value);
        }
        
        [Secure, JsonProperty(PropertyName="outboundEmail")]
        public string OutboundEmail {
            get => Get<string>("outboundEmail");
            set => Set("outboundEmail", value);
        }

        [Secure, JsonProperty(PropertyName="outboundEmailId")]
        public int OutboundEmailId {
            get => Get<int>("outboundEmailId");
            set => Set("outboundEmailId", value);
        }

        [JsonProperty(PropertyName="outboundEmailBCC")]
        public bool OutboundEmailBCC {
            get => Get<bool>("outboundEmailBCC");
            set => Set("outboundEmailBCC", value);
        }

        [JsonProperty(PropertyName="firstName")]
        public string FirstName {
            get => Get<string>("firstName");
            set => Set("firstName", value);
        }

        [JsonProperty(PropertyName="lastName")]
        public string LastName {
            get => Get<string>("lastName");
            set => Set("lastName", value);
        }

        [Secure, JsonProperty(PropertyName="photo")]
        public string Photo {
            get => Get<string>("photo");
            set => Set("photo", value);
        }

        [Secure, JsonProperty(PropertyName="lastLogin")]
        public DateTime? LastLogin {
            get => Get<DateTime?>("lastLogin");
            set => Set("lastLogin", value);
        }

        [JsonProperty(PropertyName="signature")]
        public string Signature {
            get => Get<string>("signature");
            set => Set("signature", value);
        }

        [Secure, JsonProperty(PropertyName="role")]
        public string Role {
            get => Get<string>("role");
            set => Set("role", value);
        }
        
        [Secure, JsonProperty(PropertyName="groups")]
        public List<string> Groups {
            get => Get<List<string>>("groups");
            set => Set("groups", value);
        }

        [Secure, JsonProperty(PropertyName="policies")]
        public List<CosmosPolicy> Policies {
            get => Get<List<CosmosPolicy>>("policies");
            set => Set("policies", value);
        }

        [Secure, JsonProperty(PropertyName="components")]
        public Dictionary<string, object> Components {
            get => Get<Dictionary<string, object>>("components");
            set => Set("components", value);
        }
    }
}