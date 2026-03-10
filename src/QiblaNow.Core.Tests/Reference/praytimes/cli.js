const { PrayTime } = require('./praytime');

function parseArgs(argv) {
    const args = {};
    for (let i = 2; i < argv.length; i++) {
        const a = argv[i];
        if (!a.startsWith('--')) continue;
        const key = a.slice(2);
        const next = argv[i + 1];
        if (!next || next.startsWith('--')) {
            args[key] = true;
        } else {
            args[key] = next;
            i++;
        }
    }
    return args;
}

function pad(n) {
    return String(n).padStart(2, '0');
}

function parseDate(value) {
    const [y, m, d] = value.split('-').map(Number);
    return new Date(Date.UTC(y, m - 1, d));
}

function daysInMonth(year, month) {
    return new Date(year, month, 0).getDate();
}

function methodName(value) {
    const map = {
        MWL: 'MWL',
        MuslimWorldLeague: 'MWL',
        ISNA: 'ISNA',
        Egypt: 'Egypt',
        Makkah: 'Makkah',
        Karachi: 'Karachi',
        Tehran: 'Tehran',
        Jafari: 'Jafari',
        France: 'France',
        Russia: 'Russia',
        Singapore: 'Singapore'
    };
    return map[value] || value;
}

function asrName(value) {
    if (!value) return 'Standard';
    const v = String(value).toLowerCase();
    if (v === 'hanafi') return 'Hanafi';
    return 'Standard';
}

function highLatName(value) {
    const map = {
        none: 'None',
        middleofnight: 'NightMiddle',
        nightmiddle: 'NightMiddle',
        seventhofnight: 'OneSeventh',
        oneseventh: 'OneSeventh',
        anglebased: 'AngleBased'
    };
    const key = String(value || 'middleofnight').toLowerCase();
    return map[key] || value;
}

function buildCalculator(args) {
    const pt = new PrayTime(methodName(args.method || 'MWL'));

    pt.location([Number(args.lat), Number(args.lon)]);
    pt.timezone(args.timezone || 'UTC');
    pt.format(args.format || '24h');
    pt.round(args.rounding || 'nearest');

    pt.adjust({
        asr: asrName(args.asr),
        highLats: highLatName(args.highLats)
    });

    if (args.dhuhrOffset) {
        pt.adjust({ dhuhr: `${Number(args.dhuhrOffset)} min` });
    }

    const tune = {};
    const tunables = ['fajr', 'sunrise', 'dhuhr', 'asr', 'sunset', 'maghrib', 'isha'];
    for (const key of tunables) {
        const argName = `${key}Offset`;
        if (args[argName] !== undefined) {
            tune[key] = Number(args[argName]);
        }
    }
    if (Object.keys(tune).length > 0) {
        pt.tune(tune);
    }

    return pt;
}

function toRow(day, times) {
    return {
        day,
        fajr: times.fajr,
        sunrise: times.sunrise,
        dhuhr: times.dhuhr,
        asr: times.asr,
        maghrib: times.maghrib,
        isha: times.isha
    };
}

function printTable(rows) {
    console.log('Day\tFajr\tSunrise\tDhuhr\tAsr\tMaghrib\tIsha');
    for (const r of rows) {
        console.log(
            `${r.day}\t${r.fajr}\t${r.sunrise}\t${r.dhuhr}\t${r.asr}\t${r.maghrib}\t${r.isha}`
        );
    }
}

function main() {
    const args = parseArgs(process.argv);
    const pt = buildCalculator(args);

    if (args.date) {
        const date = parseDate(args.date);
        const times = pt.getTimes(date);
        const row = toRow(args.date, times);

        if (args.json) {
            console.log(JSON.stringify(row, null, 2));
        } else {
            printTable([row]);
        }
        return;
    }

    if (args.month) {
        const [y, m] = args.month.split('-').map(Number);
        const count = daysInMonth(y, m);
        const rows = [];

        for (let d = 1; d <= count; d++) {
            const date = new Date(Date.UTC(y, m - 1, d));
            const times = pt.getTimes(date);
            rows.push(toRow(d, times));
        }

        if (args.json) {
            console.log(JSON.stringify(rows, null, 2));
        } else {
            printTable(rows);
        }
        return;
    }

    console.error('Use --date YYYY-MM-DD or --month YYYY-MM');
    process.exit(1);
}

main();