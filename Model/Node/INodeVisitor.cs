namespace AngelNode.Model.Node
{
    public interface INodeVisitor
    {
        void VisitNodeDialogue(NodeDialogue nd);
        void VisitNodeFadeMusic(NodeFadeMusic ndm);
        void VisitNodeChangeBackground(NodeChangeBackground ncb);
        void VisitNodeChangePose(NodeChangePose ncp);
        void VisitNodeJump(NodeJump nj);
        void VisitNodeMovement(NodeMovement nm);
        void VisitNodeCall(NodeCall nc);
        void VisitNodeRet(NodeRet nr);
        void VisitNodePlaySound(NodePlaySound nps);
        void VisitNodeResponseDialogue(NodeResponseDialogue nrd);
        void VisitNodeSetVariable(NodeSetVariable nsv);
        void VisitNodeShake(NodeShake ns);
        void VisitNodeVariableJump(NodeVariableJump nvj);
        void VisitNodeWait(NodeWait nw);
        void VisitNodeEvent(NodeEvent ne);
        void VisitNodeTodo(NodeTodo nt);
        void VisitNodePhone(NodePhone np);
        void VisitNodeLua(NodeLua nl);
        void VisitNodeAchievement(NodeAchievement na);
        void VisitNodeRouteCompleted(NodeRouteCompleted nrc);
        void VisitNodeOutfitUnlocked(NodeOutfitUnlocked nou);
    }
}
