// Inherited by: Dog, Human
interface IAnimalActions
{
    string Talk();
    (int, int) Move(int direction, int seconds);
}

// Inherited by: Human
interface IHumanActions
{
    bool GoToSchool(int price);
    void GraduateSchool();
    void GoToWork();
    bool BuyPet(Pet pet, int price);
    bool BuyCar(int price, int speed, int fuelCapacity, int fuelUsedPerMove);
    void SellCar();
    bool FillCarFuelTank();
}

// Inherited by: Pet, Human
abstract class Animal
{
    public int LocationX { get; internal set; }
    public int LocationY { get; internal set; }
    public int Speed { get; }
    public int Age { get; }

    public Animal(int locationX, int locationY, int speed)
    {
        LocationX = locationX;
        LocationY = locationY;
        Speed = speed;
    }

    public virtual (int, int) Move(int direction, int seconds)
    {
        if (direction == 0) // move north
            LocationY += seconds * Speed;
        else if (direction == 1) // move east
            LocationX += seconds * Speed;
        else if (direction == 2) // move south
            LocationY -= seconds * Speed;
        else if (direction == 3) // move west
            LocationX -= seconds * Speed;
        return (LocationX, LocationY);
    }
}

// Inherits: Animal
// Inherited by: Dog
// Aggregated by: Human
// Used by: Human
abstract class Pet : Animal
{
    public int Cost { get; }
    public Pet(int locationX, int locationY, int speed, int price) : base(locationX, locationY, speed)
    {
        Cost = price;
    }
}

// Inherits: Pet, IAnimalActions
// Aggregated by: Human
class Dog : Pet, IAnimalActions
{
    public string Breed { get; }

    public Dog(int locationX, int locationY, int speed, int price, string breed) : base(locationX, locationY, speed, price)
    {
        Breed = breed;
    }

    public string Talk() => "Woof!";
}

// Inherits: Animal, IAnimalActions, IHumanActions
// Aggregates: Car, Pet, Dog
// Uses: Pet
class Human : Animal, IAnimalActions, IHumanActions
{
    public int Money { get; internal set; }

    private Car car = null;
    public List<Pet> Pets { get; }

    public int DailySalary { get; internal set; }
    public bool IsDriving { get; internal set; }
    public bool IsInSchool { get; internal set; }

    public Human(int locationX, int locationY, int speed, int money, int dailySalary) : base(locationX, locationY, speed)
    {
        Money = money;
        DailySalary = dailySalary;
    }

    public string Talk() => "Hello";

    public override (int, int) Move(int direction, int seconds)
    {
        if (direction == 0) // move north
            LocationY += seconds * Speed;
        else if (direction == 1) // move east
            LocationX += seconds * Speed;
        else if (direction == 2) // move south
            LocationY -= seconds * Speed;
        else if (direction == 3) // move west
            LocationX -= seconds * Speed;
        return (LocationX, LocationY);
    }

    public bool GoToSchool(int price)
    {
        if (Money >= price)
        {
            Money -= price;
            return IsInSchool = true;
        }
        return false;
    }

    public void GraduateSchool()
    {
        if (IsInSchool)
        {
            DailySalary += (int)(DailySalary * 0.2);
            IsInSchool = false;
        }
    }

    public void GoToWork()
    {
        int earnings = DailySalary;
        if (IsInSchool)
            earnings -= (int)(DailySalary * 0.5);
        Money += earnings;
    }

    public bool BuyPet(Pet pet, int price)
    {
        if (Money >= price)
        {
            Money -= price;
            Pets.Add(pet);
            return true;
        }
        return false;
    }

    public bool BuyDog(int locationX, int locationY, int speed, int price, string breed)
    {
        Dog dog = new Dog(locationX, locationY, speed, price, breed);
        if (BuyPet(dog, price))
            return true;
        return false;
    }

    public bool BuyCar(int price, int speed, int fuelCapacity, int fuelUsedPerMove)
    {
        if (Money >= price)
        {
            Money -= price;
            car = new Car(price, speed, fuelCapacity, fuelCapacity, fuelUsedPerMove);
            IsDriving = true;
            return true;
        }
        return false;
    }

    public void SellCar()
    {
        if (car == null) return;

        Money += (int)(car.Price * 0.5);
        car = null;
        IsDriving = false;
    }

    public bool FillCarFuelTank()
    {
        if (car == null) return false;

        int cost = car.FillTank(Money);

        if (cost == 0) return false;

        Money -= cost;
        return true;
    }
}

// Aggregated by: Human
class Car
{
    private static int priceOfFuel = 5;
    public int Price { get; set; }
    public int Speed { get; set; }

    public double FuelTankStatus
    {
        get => fuelLevel / fuelCapacity;
    }

    public int MoveTimeLeftBeforeEmpty
    {
        get => (int)(fuelLevel / (Speed * fuelUsedPerMove));
    }

    public int MoveDistanceLeftBeforeEmpty
    {
        get => (int)(MoveTimeLeftBeforeEmpty * Speed);
    }

    private int fuelCapacity;
    private double fuelLevel;
    private double fuelUsedPerMove;

    public Car(int price, int speed, int fuelCapacity, int fuelLevel, int fuelUsedPerMove)
    {
        Price = price;
        Speed = speed;
        this.fuelCapacity = fuelCapacity;
        this.fuelLevel = fuelLevel;
        this.fuelUsedPerMove = fuelUsedPerMove;
    }

    /* Returns the amount of money used */
    public int FillTank(int money)
    {
        int cost = (int)(fuelCapacity - fuelLevel) * priceOfFuel;
        if (money >= cost)
        {
            fuelLevel += priceOfFuel * cost;
            return cost;
        }
        return 0;
    }
}