namespace ElevatorForBaseArea51 {
    using System;
    using System.Threading;

    public class Base51 {

        public const int elevatorTimePerFloor = 1000;

        public static Semaphore elevatorSemaphore;

        public static Random rand = new Random();

        public static string[] floors = { "G", "S", "T1", "T2" };

        public static int currentElevatorFloor;

        public static void Main() {
            Console.WriteLine("Press ESC to cancel the process");


            currentElevatorFloor = 0;
            // 1 ,1 защото даваме да допускаме само по една нишка 
            elevatorSemaphore = new Semaphore(1, 1);

            Console.WriteLine("Create agents");
            //създавам си агенти 
            var agentsCount = rand.Next(3, 6);
            //създавамемасив от броя на агентите
            var agents = new Agent[agentsCount];
            //обхождам масива и създавам нов агент и попълвам масива с тези агенти
            for (int i = 0; i < agentsCount; i++) {
                var agent = new Agent();
                agent.id = rand.Next(1, 20);
                //на кой етаж се намира
                agent.currentFloor = rand.Next(0, floors.Length);
                //случайно си генерирам секиурити левела и ще върне число от 0 до 2 
                agent.securityLevelAccess = (SecurityLevel)rand.Next(0, 3);
                //вкарвам в масива агента
                agents[i] = agent;
            }

            Console.WriteLine("Activating agents");
            //пускаме нишките
            var cts = new CancellationTokenSource();
            //създавам масив от тредове
            var agentThreads = new Thread[agents.Length];
            //попълвам го
            for (int i = 0; i < agentThreads.Length; i++) {
                agentThreads[i] = new Thread(agents[i].Run);
                agentThreads[i].Start(cts.Token);
            }
            char pressedKey = ' ';
            while (pressedKey != (char)ConsoleKey.Escape) {
                //изчаква докато се натиске ескейп и проверява дали да спре и се върти докато не натиснем ескейп
                //отделно спираме програмата и спираме нишките

                pressedKey = Console.ReadKey().KeyChar;
                if (pressedKey == (char)ConsoleKey.Escape) {

                    Console.WriteLine("Stopping");
                    // казва на нишките да спрат първо 
                    //и след това се сприра и целия процес

                    cts.Cancel();
                    // обикалям всички трейдове и се изчакват преди да продължат
                    foreach (var agentThread in agentThreads) {
                        agentThread.Join();
                    }

                }
            }
        }
    }

    public class Agent {
        public int id;
        public int currentFloor;
        public SecurityLevel securityLevelAccess;
        // пускам нишката и сe влиза в безкраен цикъл(  усе елеватор)
        public void Run(object tag) {
            CancellationToken token = (CancellationToken)tag;

            while (true) {
                //семафора изчаква за да не влезнат всички нишки заедно
                Base51.elevatorSemaphore.WaitOne();
                //нишката преди да влезне в асансжора пита дали е освободено
                if (token.IsCancellationRequested) {
                    //освобождава семфора за заключване,което значи че нишката пе приключила
                    //работа и е сводобо да влезне другатаа
                    Base51.elevatorSemaphore.Release();
                    break;
                }
                //заключваме за да може само един агент да използва асансьора
                UseElevator(token);

                Base51.elevatorSemaphore.Release();
            }
        }

        public void UseElevator(CancellationToken token) {
            Console.WriteLine();
            Console.WriteLine("-----------------");
            Console.WriteLine();

            Console.WriteLine("Agent: " + this.id + ", access level: " + this.securityLevelAccess.ToString());
            Console.WriteLine("Calling elevator from floor " + currentFloor);

            //спирам дадената нишка за известно време

            Thread.Sleep(Math.Abs(Base51.currentElevatorFloor - currentFloor) * Base51.elevatorTimePerFloor);

            //етажа на който се намира асансьора заменя стойността си с етажа,на който се намира агента
            Base51.currentElevatorFloor = currentFloor;
            //казва на кой етаж е стигнал асанс
            Console.WriteLine("Elevator reached floor " + Base51.currentElevatorFloor);
            //агента влиза в асан
            //влизаме в безкраен цикъл 
            while (true) {
                if (token.IsCancellationRequested) {
                    break;
                }
                //стига до някакъв етаж и директно си правим проверка дали този етаж е достъпен,
                //ако може излиза от цикъла 
                //ако не може продължава да върти в цикула 
                //генерираме рандом число,на кой число иска да отиде
                int randomFloorToGoTo = Base51.rand.Next(0, Base51.floors.Length);
                //изписвам идто на агента ,който е избрал даден етаж 
                Console.WriteLine("Agent " + this.id + " chose floor " + randomFloorToGoTo);
                //слийпваме трейда за да стигне до този етаж

                Thread.Sleep(Math.Abs(Base51.currentElevatorFloor - randomFloorToGoTo) * Base51.elevatorTimePerFloor);
                //сменяме настоящия етаж с етажа на който иска агента да стигне
                Base51.currentElevatorFloor = randomFloorToGoTo;
                currentFloor = randomFloorToGoTo;
                //когато стигне изписва, че е стигнал на този етаж с този агент
                Console.WriteLine("Elevator reached with agent " + this.id + " floor " + Base51.currentElevatorFloor);
                //тук проверявам дали има достъп дадения агент

                if (IsFloorAccessible(this.securityLevelAccess, Base51.currentElevatorFloor)) {
                    Console.WriteLine("Agent " + this.id + " got off at floor " + currentFloor);
                    break;
                }
                else {
                    Console.WriteLine("ACCESS DENIED for floor " + currentFloor);
                }
                Console.WriteLine();
            }
        }

        public static bool IsFloorAccessible(SecurityLevel securityLevelAccess, int floorLevel) {
            // застъпвам масива по горе за етажите и проверяваме дали си съвпадат
            switch (securityLevelAccess) {
                case SecurityLevel.Confidential: {
                        if (Base51.floors[floorLevel] == "G") {
                            return true;
                        }
                        else {
                            return false;
                        }

                        
                    }
                case SecurityLevel.Secret: {
                        if (Base51.floors[floorLevel] == "G" || Base51.floors[floorLevel] == "S") {
                            return true;
                        }
                        else {
                            return false;
                        }

                        
                    }
                case SecurityLevel.TopSecret: {
                        return true;
                        
                    }
                default: {
                        return false;
                    }
            }
        }
    }
    //типа на секиурити аксеса
    public enum SecurityLevel {
        Confidential,
        Secret,
        TopSecret
    }
}