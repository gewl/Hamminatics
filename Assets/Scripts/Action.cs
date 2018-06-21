public struct Action {

    public CardData card;
    public EntityData entity;
    public Direction direction;
    public int distance;

    public Action(CardData _card, EntityData _entity, Direction _direction, int _distance)
    {
        card = _card;
        entity = _entity;
        direction = _direction;
        distance = _distance;
    }
}
