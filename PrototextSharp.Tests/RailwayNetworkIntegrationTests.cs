namespace PrototextSharp.Tests;

public class RailwayNetworkIntegrationTests
{
    private ProtoNode _root = null!;
    private List<ProtoNode> _services = null!;

    /// <summary>
    /// Loads and parses the complete railway network fixture once for the integration test class.
    /// </summary>
    [OneTimeSetUp]
    public void ParseFixture()
    {
        string fixturePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "railway-network.prototext");
        string prototext = File.ReadAllText(fixturePath);

        _root = new PrototextParser().Parse(prototext);
        _services = ((List<object?>)_root["service"]!).Cast<ProtoNode>().ToList();
    }

    /// <summary>
    /// Verifies that railway identity, dispatch settings, and maintenance paths can be extracted from the root message.
    /// </summary>
    [Test]
    public void ParseFixture_ExtractsRailwayIdentityAndDispatchSettings()
    {
        ProtoNode railway = (ProtoNode)_root["railway"]!;
        ProtoNode dispatch = (ProtoNode)railway["dispatch_center"]!;
        ProtoNode policy = (ProtoNode)railway["maintenance_policy"]!;

        Assert.Multiple(() =>
        {
            Assert.That(railway["network_name"], Is.EqualTo("Silver Meridian"));
            Assert.That(railway["operating_region"], Is.EqualTo("North Corridor"));
            Assert.That(dispatch["call_sign"], Is.EqualTo("MILEPOST-7"));
            Assert.That(dispatch["control_port"], Is.EqualTo(17420));
            Assert.That(dispatch["emergency_channel"], Is.EqualTo("RM-452"));
            Assert.That(dispatch["overnight_schedule"], Is.EqualTo(false));
            Assert.That(policy["inspection_interval_hours"], Is.EqualTo(720));
            Assert.That(policy["spare_parts_catalog"], Is.EqualTo("/opt/silver-meridian/catalog/parts.yaml"));
        });
    }

    /// <summary>
    /// Verifies that only scheduled services are returned and that their source order is retained.
    /// </summary>
    [Test]
    public void ParseFixture_ReturnsOnlyScheduledServicesInSourceOrder()
    {
        string[] serviceIds = _services.Select(GetServiceId).ToArray();

        Assert.That(serviceIds, Is.EqualTo(new[]
        {
            "EASTBOUND-314",
            "NORTHBOUND-207",
            "SHUTTLE-099",
            "NIGHT-FREIGHT-441",
            "LOCAL-028"
        }));

        Assert.That(serviceIds, Does.Not.Contain("WESTBOUND-315"));
    }

    /// <summary>
    /// Verifies that representative service, consist, locomotive, telemetry, scalar, and enum values are preserved exactly.
    /// </summary>
    [Test]
    public void ParseFixture_ExtractsRepresentativeServiceAndTelemetryValues()
    {
        ProtoNode service = FindService("EASTBOUND-314");
        ProtoNode consist = (ProtoNode)service["consist"]!;
        ProtoNode locomotive = (ProtoNode)consist["locomotive"]!;
        ProtoNode telemetry = (ProtoNode)service["telemetry"]!;

        Assert.Multiple(() =>
        {
            Assert.That(service["route_name"], Is.EqualTo("Harbor Loop"));
            Assert.That(service["direction"], Is.EqualTo("DIRECTION_EAST"));
            Assert.That(service["priority"], Is.EqualTo(2));
            Assert.That(consist["passenger_cars"], Is.EqualTo(6));
            Assert.That(consist["dining_car"], Is.EqualTo(true));
            Assert.That(locomotive["fleet_number"], Is.EqualTo("M-882"));
            Assert.That(locomotive["traction_mode"], Is.EqualTo("TRACTION_ELECTRIC"));
            Assert.That(locomotive["commissioned_year"], Is.EqualTo(2018));
            Assert.That(telemetry["gateway_address"], Is.EqualTo("fd31:7c2e:9100::314"));
            Assert.That(telemetry["sample_period_seconds"], Is.EqualTo(15));
        });
    }

    /// <summary>
    /// Verifies that repeated timetable stop messages can be extracted in their original order.
    /// </summary>
    [Test]
    public void ParseFixture_ExtractsTimetableStopsInSourceOrder()
    {
        ProtoNode service = FindService("EASTBOUND-314");
        ProtoNode timetable = (ProtoNode)service["timetable"]!;
        List<ProtoNode> stops = ((List<object?>)timetable["stops"]!).Cast<ProtoNode>().ToList();

        Assert.That(stops.Select(stop => stop["station_code"]), Is.EqualTo(new[]
        {
            "BRIAR-POINT", "LANTERN-CROSS", "COPPER-FALLS"
        }));

        Assert.That(stops.Select(stop => stop["platform"]), Is.EqualTo(new[] { 2, 1, 4 }));
    }

    /// <summary>
    /// Verifies that an empty operator notes message produces an empty child node.
    /// </summary>
    [Test]
    public void ParseFixture_EmptyOperatorNotesProducesEmptyNode()
    {
        ProtoNode service = FindService("NIGHT-FREIGHT-441");
        ProtoNode timetable = (ProtoNode)service["timetable"]!;
        ProtoNode notes = (ProtoNode)timetable["operator_notes"]!;

        Assert.That(notes.Fields, Is.Empty);
    }

    /// <summary>
    /// Verifies that battery traction and optional consist flags can be extracted when present.
    /// </summary>
    [Test]
    public void ParseFixture_ExtractsBatteryLocomotiveAndOptionalFields()
    {
        ProtoNode service = FindService("SHUTTLE-099");
        ProtoNode consist = (ProtoNode)service["consist"]!;
        ProtoNode locomotive = (ProtoNode)consist["locomotive"]!;

        Assert.Multiple(() =>
        {
            Assert.That(locomotive["model"], Is.EqualTo("Ember-2"));
            Assert.That(locomotive["traction_mode"], Is.EqualTo("TRACTION_BATTERY"));
            Assert.That(consist["accessibility_car"], Is.EqualTo(true));
            Assert.That(consist["dining_car"], Is.EqualTo(false));
        });
    }

    private ProtoNode FindService(string serviceId)
    {
        return _services.Single(service => GetServiceId(service) == serviceId);
    }

    private static string GetServiceId(ProtoNode service)
    {
        return (string)service["service_id"]!;
    }
}
