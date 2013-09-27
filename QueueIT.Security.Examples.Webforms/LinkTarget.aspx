<%@ Page Title="Link" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LinkTarget.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms.LinkTarget" %>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>A Known User implementation of a queue.</h2>
            </hgroup>
            <p>
                To learn more about configuring queues, please contact Queue-it.
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Setting up the queue:</h3>
    <ol class="round">
        <li class="one">
            <h5>Write Known User code</h5>
            Add Known User code to the PreInit event. The eventhandler of the LinkTarget.aspx 
            page contains code to extract and persist information about a queue number. You should place this 
            code in the page PreInit event.</li>
    </ol>
</asp:Content>
